using DUST.Data;
using DUST.Models;
using DUST.Models.Enums;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly IRolesService _rolesService;

        public ProjectService(ApplicationDbContext context, UserManager<DUSTUser> userManager, IRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
        }

        // CRUD - Add
        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            DUSTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Error removing current PM. - {ex.Message}");
                    return false;
                }
            }

            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding a new PM. - Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            bool result = false;

            DUSTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (! await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
                        result = true; 
                    } 
                    catch(Exception)
                    {
                        throw;
                    }
                }
            }

            return result;
        }
        
        // CRUD - Archive (Delete)
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DUSTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<DUSTUser> developers = await GetProjectMembersByRoleAsync(projectId, RolesEnum.Developer.ToString());
            List<DUSTUser> submitters = await GetProjectMembersByRoleAsync(projectId, RolesEnum.Submitter.ToString());
            List<DUSTUser> admin = await GetProjectMembersByRoleAsync(projectId, RolesEnum.Admin.ToString());

            List<DUSTUser> membersExceptPM = developers.Concat(submitters).Concat(admin).ToList();

            return membersExceptPM; 
        }

        /// <summary>
        /// Returns all projects in a company **except archived projects**
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetAllActiveProjectsByCompanyAsync (int companyId)
        {
            List<Project> projects = new();

            projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllActiveProjectsByCompanyAsync(companyId);
            int priorityId = await LookupProjectPriorityIdAsync(priorityName);

            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();

            // Why not do this?
            //return projects.Where(p => p.ProjectPriority.Name == priorityName).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();
            return projects;
        }

        public Task<List<DUSTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }
        
        // CRUD - Read
        public async Task<Project> GetProjectByIdAsync(int companyId, int projectId)
        {
            Project project = await _context.Projects
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Members)
                                            .Include(p => p.ProjectPriority)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

            return project;
        }

        public async Task<DUSTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            foreach(DUSTUser member in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(member,RolesEnum.ProjectManager.ToString()))
                {
                    return member;
                }
            }
            return null;

            //DUSTUser projectManager = (await GetProjectMembersByRoleAsync(projectId, Roles.ProjectManager.ToString())).First();
        }

        public async Task<List<DUSTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            List<DUSTUser> members = new();

            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null)
            {
                foreach (var user in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, role))                       
                    {
                        members.Add(user);
                    }
                }                  
            }
            return members;
        }

        public Task<List<DUSTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Company)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Members)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.DeveloperUser)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.OwnerUser)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.TicketPriority)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.TicketStatus)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.TicketType)
                    .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                return userProjects;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** -Error getting user projects. ---> {ex.Message}");
                throw;
            }
        }

        public async Task<List<DUSTUser>> GetUsersNotOnProjectAsync(int companyId, int projectId)
        {
            List<DUSTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();

            return users.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            bool result = false;
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            
            if (project != null)
            {
                result = project.Members.Any(m => m.Id == userId);
            }

            return result;
        }

        public async Task<int> LookupProjectPriorityIdAsync(string priorityName)
        {
            int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
            return priorityId;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            DUSTUser projectManager = await GetProjectManagerAsync(projectId);

            if (projectManager != null)
            {
                await RemoveUserFromProjectAsync(projectManager.Id, projectId);
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                DUSTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                } 
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Removing user from project failed. ---> {ex.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(int projectId, string role)
        {
            try
            {
                List<DUSTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (DUSTUser user in members)
                {
                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** -Error removing users from projects by role. ---> {ex.Message}");
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // CRUD - Update
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
