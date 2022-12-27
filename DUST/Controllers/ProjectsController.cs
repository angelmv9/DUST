using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DUST.Data;
using DUST.Models;
using DUST.Extensions;
using DUST.Models.ViewModels;
using DUST.Services.Interfaces;
using DUST.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using DUST.Services;

namespace DUST.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        #region Member variables
        private readonly IRolesService _rolesService;
        private readonly ILookupService _lookupService;
        private readonly IFilesService _fileService;
        private readonly IProjectService _projectService;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly ICompanyInfoService _companyService;
        #endregion

        #region Constructor
        public ProjectsController(IRolesService rolesService,
            ILookupService lookupService,
            IFilesService fileService,
            IProjectService projectService,
            UserManager<DUSTUser> userManager,
            ICompanyInfoService companyService)
        {
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyService = companyService;
        } 
        #endregion

        #region Index Views
        
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();
            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(RolesEnum.Admin)) || User.IsInRole(nameof(RolesEnum.ProjectManager)))
            {
                projects = await _companyService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllActiveProjectsByCompanyAsync(companyId);
            }

            return View(projects);
        }

        public async Task<IActionResult> ArchivedProjects()
        {
            List<Project> archivedProjects = new();
            int companyId = User.Identity.GetCompanyId().Value;

            archivedProjects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);
 
            return View(archivedProjects);
        }

        [Authorize(Roles = nameof(RolesEnum.Admin))]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> unassignedProjects = new();
            unassignedProjects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(unassignedProjects);
        }

        #endregion

        #region Details
        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(companyId, id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
        #endregion

        #region Create
        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<DUSTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(RolesEnum.ProjectManager.ToString(), companyId);
            List<ProjectPriority> projectPriorities = await _lookupService.GetProjectPrioritiesAsync();

            AddProjectWithPMViewModel model = new();
            model.PMList = new SelectList(projectManagers, "Id", "FullName");
            model.PriorityList = new SelectList(projectPriorities, "Id", "Name");

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.FileExtention = model.Project.ImageFormFile.ContentType;
                    }
                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        bool isPMAdded = await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                        if (!isPMAdded)
                        {
                            Debug.WriteLine("Error adding a project manager while creating a project.");
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                // TODO: redirect to All Projects
                return RedirectToAction("Index");
            }

            return RedirectToAction("Create");
        }
        #endregion

        #region Edit
        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
			int companyId = User.Identity.GetCompanyId().Value;
			bool isProjectArchived = (await _projectService.GetProjectByIdAsync(companyId, id.Value)).Archived;

			if (id == null || isProjectArchived)
            {
                return NotFound();
            }

            List<DUSTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(RolesEnum.ProjectManager.ToString(), companyId);
            List<ProjectPriority> projectPriorities = await _lookupService.GetProjectPrioritiesAsync();
            DUSTUser PM = await _projectService.GetProjectManagerAsync(id.Value);

            AddProjectWithPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(companyId, id.Value);
            model.PMList = new SelectList(projectManagers, "Id", "FullName", PM?.Id);
            model.PriorityList = new SelectList(projectPriorities, "Id", "Name", model.Project?.ProjectPriority?.Name);

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.FileExtention = model.Project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        bool isPMAdded = await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                        if (!isPMAdded)
                        {
                            Debug.WriteLine("Error adding a project manager while creating a project.");
                        }
                    }
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction("Edit");
        }
        #endregion

        #region Archive
        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(companyId, id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(companyId, id);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Manage Team
        //Get: Projects/ManageTeam/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            ProjectMembersViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(companyId, projectId);

            List<DUSTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(RolesEnum.Developer), companyId);
            List<DUSTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(RolesEnum.Submitter), companyId);
            List<DUSTUser> companyMembers = developers.Concat(submitters).ToList();
            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers, "Id", "FullName",projectMembers);

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (model.SelectedUsersIds != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id)).Select(m => m.Id).ToList();
                foreach (string memberId in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(memberId, model.Project.Id);
                }
                foreach (string userId in model.SelectedUsersIds)
                {
                    await _projectService.AddUserToProjectAsync(userId, model.Project.Id);
                }

                return RedirectToAction("Details", new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignMembers), new {projectId = model.Project.Id});
        }
        #endregion

        #region Assign PM
        //Get: Projects/AssignPM/3
        [Authorize(Roles = nameof(RolesEnum.Admin))]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            AssignPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(companyId, projectId);

            List<DUSTUser> PMs = await _rolesService.GetUsersInRoleAsync(nameof(RolesEnum.ProjectManager), companyId);
            model.ProjectManagers = new SelectList(PMs, "Id", "FullName");

            return View(model);
        }

        //Post: Projects/AssignPM/3
        [Authorize(Roles = nameof(RolesEnum.Admin))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                return RedirectToAction(nameof(Details), new {id = model.Project.Id});         
            }

            return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
        }
        #endregion

        #region Restore
        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(companyId, id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(companyId, id);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }
        #endregion

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            return (await _projectService.GetAllActiveProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);
        }
    }
}
