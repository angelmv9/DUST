using DUST.Models.ViewModels;
using DUST.Services.Interfaces;
using DUST.Extensions;
using DUST.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DUST.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IRolesService _rolesService;
        private readonly ICompanyInfoService _companyInfoService;

        public UserRolesController(IRolesService rolesService, ICompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }

        // GET: UserRoles/ManageUserRoles
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<DUSTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (DUSTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.DUSTUser = user;

                IEnumerable<string> currentlySelectedRoles = await _rolesService.GetUserRolesAsync(user);
                List<IdentityRole> allPossibleRoles = await _rolesService.GetRolesAsync();
                viewModel.Roles = new MultiSelectList(allPossibleRoles, "Name", "Name", currentlySelectedRoles);

                model.Add(viewModel);
            }

            // It generates an HTML template to be displayed in the browser without making a new request.
            return View(model);
        }

        // POST: UserRoles/ManageUserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            DUSTUser user = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.DUSTUser.Id);
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(user);

            // Currently, only one role per user is allowed, other than the seeded roles for demo
            // purposes. If multiple roles are a future feature, update newRole to List<string> newRoles
            string newRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(newRole))
            {
                if (await _rolesService.RemoveUserFromRolesAsync(user, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(user, newRole);
                }
            }

            // Makes the browser redirects to a different action, thus making another request
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
