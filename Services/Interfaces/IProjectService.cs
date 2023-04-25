using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DUST.Models;

namespace DUST.Services.Interfaces
{
    public interface IProjectService
    {
        public Task AddNewProjectAsync(Project project);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);
        public Task<bool> AddUserToProjectAsync(string userId, int projectId);
        public Task ArchiveProjectAsync(Project project);
        /// <summary>
        /// Returns all projects in a company **except archived projects**
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Task<List<Project>> GetAllActiveProjectsByCompanyAsync(int companyId);
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName);
        public Task<List<DUSTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        public Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId);
        public Task<List<DUSTUser>> GetDevelopersOnProjectAsync(int projectId);
        public Task<DUSTUser> GetProjectManagerAsync(int projectId);
        public Task<List<DUSTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        public Task<Project> GetProjectByIdAsync(int companyId, int projectId);
        public Task<List<DUSTUser>> GetSubmittersOnProjectAsync(int projectId);        
        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);
        public Task<List<DUSTUser>> GetUsersNotOnProjectAsync(int companyId, int projectId);
        public Task<List<Project>> GetUserProjectsAsync(string userId);
        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        public Task<int> LookupProjectPriorityIdAsync(string priorityName);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task RemoveUsersFromProjectByRoleAsync(int projectId, string role);
        public Task RemoveUserFromProjectAsync(string userId, int projectId);
        public Task RestoreProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);
    }
}
