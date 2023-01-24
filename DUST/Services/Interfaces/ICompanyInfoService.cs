using DUST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services.Interfaces
{
    public interface ICompanyInfoService
    {
        public Task<bool> AddNewCompanyAsync(Company company);
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);
        public Task<List<DUSTUser>> GetAllMembersAsync(int companyId);
        public Task<List<Project>> GetAllProjectsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);

        public DUSTUser GetMemberProfile(string userId, int companyId);
        public Task<int> GetCompanyIdByName(string companyName);

        public Task DeleteCompanyAsync(int companyId);
    }
}
