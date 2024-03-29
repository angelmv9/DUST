﻿using DUST.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services.Interfaces
{
    public interface IRolesService
    {
        public Task<bool> IsUserInRoleAsync(DUSTUser user, string roleName);
        public Task<List<IdentityRole>> GetRolesAsync();
        public Task<IEnumerable<string>> GetUserRolesAsync(DUSTUser user);
        public Task<bool> AddUserToRoleAsync(DUSTUser user, string roleName);
        public Task<bool> RemoveUserFromRoleAsync(DUSTUser user, string roleName);
        public Task<bool> RemoveUserFromRolesAsync(DUSTUser user, IEnumerable<string> roles);
        public Task<List<DUSTUser>> GetUsersInRoleAsync (string roleName, int companyId);
        public Task<List<DUSTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        public Task<string> GetRoleNameByIdAsync(string roleId);
    }
}
