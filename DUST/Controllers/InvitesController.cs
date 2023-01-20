using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DUST.Data;
using DUST.Models;
using DUST.Services.Interfaces;
using DUST.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using DUST.Security;
using System.Text.Encodings.Web;

namespace DUST.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectService _projectService;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly IInviteService _inviteService;
        private readonly IEmailSender _emailService;
        private readonly IDataProtector _dataProtector;

        public InvitesController(ApplicationDbContext context,
            IProjectService projectService,
            UserManager<DUSTUser> userManager,
            IInviteService inviteService,
            IEmailSender emailService,
            IDataProtectionProvider dataProtectionProvider,
            DataProtectionPurposeStrings purposeStrings)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _inviteService = inviteService;
            _emailService = emailService;
            _dataProtector = dataProtectionProvider.CreateProtector(purposeStrings.InviteLink);
        }

        // GET: Invites
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        // GET: Invites/Details/5
        //[Authorize(Roles = "Admin")]

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invite = await _context.Invites
        //        .Include(i => i.Company)
        //        .Include(i => i.Invitee)
        //        .Include(i => i.Invitor)
        //        .Include(i => i.Project)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (invite == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(invite);
        //}

        // GET: Invites/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetAllActiveProjectsByCompanyAsync(companyId);
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");

            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InvitorId,InviteeEmail,InviteeFirstName,InviteeLastName,Description")] Invite invite)
        {
            if (ModelState.IsValid)
            {
                /* Get the rest of the Invite info and save it */

                DUSTUser currentAdmin = await _userManager.GetUserAsync(User);
                invite.InvitorId = currentAdmin.Id;
                invite.CompanyId = currentAdmin.CompanyId;
                invite.InviteDate = DateTime.Now;
                Guid inviteGuid = Guid.NewGuid();
                invite.CompanyToken = inviteGuid;
                
                string defaultDescription = "You have been invited to join our company! Please follow this link to join:";
                if (string.IsNullOrEmpty(invite.Description))
                {
                    invite.Description = defaultDescription;
                }

                await _inviteService.AddNewInviteAsync(invite);

                /* Encode sensitive data to include it in the magic link */
                string protectedToken = _dataProtector.Protect(invite.CompanyToken.ToString());
                string protectedEmail = _dataProtector.Protect(invite.InviteeEmail);
                string protectedCompanyId = _dataProtector.Protect(invite.CompanyId.ToString());

                // Prepare URL (magic link)
                var callbackUrl = Url.Action(
                    "ProcessInvite",
                    "Invites",
                    new { protectedToken, protectedEmail, protectedCompanyId },
                    protocol: Request.Scheme);

                // Add magic link to the body of the message
                string message = invite.Description + $" <a href='{callbackUrl}'>JOIN!</a>";
                
                await _emailService.SendEmailAsync(invite.InviteeEmail, "From DUST: Company Invite", message);

                return RedirectToAction("Dashboard", "Home", new { message = "Invite sent successfully" });
            }
            return RedirectToAction("Create", new {message = "Something went wrong. Please try again"});
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessInvite(string protectedToken, string protectedEmail, string protectedCompanyId)
        {
            // Decode token, email and companyId in order to validate invite & get invitor information

            Guid token = new Guid(_dataProtector.Unprotect(protectedToken));
            string email = _dataProtector.Unprotect(protectedEmail);
            int companyId = int.Parse(_dataProtector.Unprotect(protectedCompanyId));

            if (! await _inviteService.ValidateInviteAsync(token))
            {
                return NotFound("We're sorry. Either this invite has been already used," +
                     " has expired or the link has been tampered with. Please contact the administrator.");
            }

            Invite invite = await _inviteService.GetInviteAsync(token, email, companyId);

            return View(invite);
        }

        // GET: Invites/Edit/5
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invite = await _context.Invites.FindAsync(id);
        //    if (invite == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
        //    ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
        //    ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
        //    return View(invite);
        //}

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,WasUsed,InviteDate,JoinDate,CompanyToken")] Invite invite)
        //{
        //    if (id != invite.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(invite);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!InviteExists(invite.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
        //    ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
        //    ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
        //    return View(invite);
        //}

        // GET: Invites/Delete/5
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var invite = await _context.Invites
        //        .Include(i => i.Company)
        //        .Include(i => i.Invitee)
        //        .Include(i => i.Invitor)
        //        .Include(i => i.Project)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (invite == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(invite);
        //}

        // POST: Invites/Delete/5
        //[Authorize(Roles = "Admin")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var invite = await _context.Invites.FindAsync(id);
        //    _context.Invites.Remove(invite);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool InviteExists(int id)
        //{
        //    return _context.Invites.Any(e => e.Id == id);
        //}
    }
}
