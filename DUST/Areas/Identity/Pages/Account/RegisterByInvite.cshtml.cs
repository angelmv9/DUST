using DUST.Models;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using DUST.Services;
using System.ComponentModel;
using DUST.Models.Enums;

namespace DUST.Areas.Identity.Pages.Account
{
    public class RegisterByInviteModel : PageModel
    {
        private readonly SignInManager<DUSTUser> _signInManager;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInviteService _inviteService;
        private readonly ICompanyInfoService _companyService;
        private readonly IProjectService _projectService;

        public RegisterByInviteModel(
            UserManager<DUSTUser> userManager,
            SignInManager<DUSTUser> signInManager,
            ILogger<RegisterModel> logger,
            IInviteService inviteService,
            IEmailSender emailSender,
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

        /* Using TempData annotation to store data (set in the OnGet) until it's read in the OnPost request*/
        [TempData]
        public int inviteId { get; set; }
        [TempData]
        public int companyId { get; set; }

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
        }

        public async Task OnGetAsync(int inviteId, int companyId, string returnUrl = null)
        {
            this.inviteId = inviteId;
            this.companyId = companyId;

            ReturnUrl = returnUrl;
            Invite invite = await _inviteService.GetInviteAsync(inviteId, companyId);
            Input = new InputModel
            {
                FirstName = invite.InviteeFirstName,
                LastName = invite.InviteeLastName,
                Email = invite.InviteeEmail,
                CompanyName = invite.Company.Name
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                DUSTUser newUser = new DUSTUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    CompanyId = this.companyId
                };
                var result = await _userManager.CreateAsync(newUser, Input.Password);
                if (result.Succeeded)
                {
                    Invite invite = await _inviteService.GetInviteAsync(this.inviteId, this.companyId);
                    await _inviteService.MarkInviteAsUsedAsync(invite.CompanyToken, newUser.Id, this.companyId);
                    await _userManager.AddToRoleAsync(newUser, RolesEnum.Submitter.ToString());
                    await _projectService.AddUserToProjectAsync(newUser.Id, invite.ProjectId);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = newUser.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(newUser, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            return Page();           
        }
    }
}
