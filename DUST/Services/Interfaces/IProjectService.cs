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
        public Task<List<Project>> GetAllProjectsByCompany(int companyId);
        public Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName);
        public Task<List<DUSTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        public Task<List<Project>> GetArchivedProjectsByCompany(int companyId);
        public Task<List<DUSTUser>> GetDevelopersOnProjectAsync(int projectId);
        public Task<DUSTUser> GetProjectManagerAsync(int projectId);
        public Task<List<DUSTUser>> GetProjectMembersByRoleAsync(int companyId, string role);
        public Task<Project> GetProjectByIdAsync(int companyId, int projectId);
        public Task<List<DUSTUser>> GetSubmittersOnProjectAsync(int projectId);
        public Task<List<DUSTUser>> GetUsersNotOnProjectAsync(int companyId, int projectId);
        public Task<List<Project>> GetUserProjectsAsync(string userId);
        public Task<bool> IsUserOnProject(string userId, int projectId);
        public Task<int> LookupProjectPriorityId(string priorityName);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task RemoveUsersFromProjectByRoleAsync(int projectId, string role);
        public Task RemoveUserFromProjectAsync(string userId, int projectId);
        public Task UpdateProjectAsync(Project project);


    }
}
