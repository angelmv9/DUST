﻿using System;
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

namespace DUST.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRolesService _rolesService;
        private readonly ILookupService _lookupService;
        private readonly IFilesService _fileService;
        private readonly IProjectService _projectService;

        public ProjectsController(ApplicationDbContext context, 
            IRolesService rolesService, 
            ILookupService lookupService, 
            IFilesService fileService, 
            IProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        #region Create
        // GET: Projects/Create
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            List<DUSTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(RolesEnum.ProjectManager.ToString(), companyId);
            List<ProjectPriority> projectPriorities = await _lookupService.GetProjectPrioritiesAsync();
            DUSTUser PM = await _projectService.GetProjectManagerAsync(id.Value);

            AddProjectWithPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(companyId, id.Value);
            model.PMList = new SelectList(projectManagers, "Id", "FullName", PM.Id);
            model.PriorityList = new SelectList(projectPriorities, "Id", "Name", model.Project?.ProjectPriority?.Name);

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                catch (Exception)
                {
                    throw;
                }
            }
            return RedirectToAction("Edit");
        }
        #endregion

        #region Delete
        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        } 
        #endregion

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
