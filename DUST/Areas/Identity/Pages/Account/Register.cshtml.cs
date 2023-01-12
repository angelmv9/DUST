﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using DUST.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using DUST.Services.Interfaces;
using DUST.Services;
using DUST.Models.Enums;

namespace DUST.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<DUSTUser> _signInManager;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInviteService _inviteService;
        private readonly ICompanyInfoService _companyService;
        private readonly IProjectService _projectService;

        public RegisterModel(
            UserManager<DUSTUser> userManager,
            SignInManager<DUSTUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IInviteService inviteService,
            ICompanyInfoService companyService,
            IProjectService projectService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _inviteService = inviteService;
            _companyService = companyService;
            _projectService = projectService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
           
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(25, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DisplayName("Company Name")]
            public string CompanyName { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must have a maximum # of {1} characters long.")]
            [DisplayName("Company Description")]
            public string CompanyDescription { get; set; }

            public int? InviteId { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null, int? inviteId = null, int? companyId = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (inviteId != null && companyId != null)
            {
                Invite invite = await _inviteService.GetInviteAsync(inviteId.Value, companyId.Value);
                Input = new InputModel
                {
                    FirstName = invite.InviteeFirstName,
                    LastName = invite.InviteeLastName,
                    Email = invite.InviteeEmail,
                    CompanyName = invite.Company.Name,
                    InviteId = invite.Id
                };              
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // A Company must be created first in order to have a user
                Company newCompany = new Company
                {
                    Name = Input.CompanyName,
                    Description = Input.CompanyDescription
                };
                bool success = await _companyService.AddNewCompanyAsync(newCompany);
                if (success)
                {
                    int companyId = await _companyService.GetCompanyIdByName(newCompany.Name);

                    var user = new DUSTUser
                    {
                        UserName = Input.Email,
                        Email = Input.Email,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        CompanyId = companyId
                    };
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        // If Registration is via Invite: 
                        if (Input.InviteId != null)
                        {
                            Invite invite = await _inviteService.GetInviteAsync(Input.InviteId.Value, companyId);
                            // Make sure the invite can't be used again
                            Guid token = invite.CompanyToken;
                            await _inviteService.AcceptInviteAsync(token, user.Id, companyId);

                            // Give the user a role of submitter.
                            await _userManager.AddToRoleAsync(user, RolesEnum.Submitter.ToString());

                            // Add the new user to the project he was invited to
                            await _projectService.AddUserToProjectAsync(user.Id, invite.ProjectId);
                        }
                        else
                        {
                            // Regular Company registration, make the user an Admin
                            await _userManager.AddToRoleAsync(user, RolesEnum.Admin.ToString());
                        }

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
