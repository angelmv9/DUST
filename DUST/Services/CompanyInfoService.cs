using DUST.Data;
using DUST.Models;
using DUST.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class CompanyInfoService : ICompanyInfoService    
    {
        private readonly ApplicationDbContext _context;

        public CompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CRUD - Add / Create
        public async Task<bool> AddNewCompanyAsync(Company company)
        {
            if (company != null) {
                try
                {
                    _context.Add(company);
                    await _context.SaveChangesAsync();

                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else { return false; }
        }

        public async Task<List<DUSTUser>> GetAllMembersAsync(int companyId)
        {
            List<DUSTUser> result = new();

            result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
            
            return result;
        }

        /// <summary>
        /// Returns all projects in a company including archived projects
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            List<Project> result = new();
            result = await _context.Projects.Where(p => p.CompanyId == companyId)
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
            return result;
        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Ticket> result = new();

            // Ticket relies on the ProjectId to relate to the Company. Otherwise it doesn't know
            // what Company it belongs to
            List<Project> projects = await GetAllProjectsAsync(companyId);

            result = projects.SelectMany(p => p.Tickets).ToList();

            return result;
            
        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company result = new();

            if (companyId != null)
            {
                result = await _context.Companies
                    .Include(c => c.Members)
                    .Include(c => c.Projects)
                    .Include(c => c.Invites)
                    .FirstOrDefaultAsync(c => c.Id == companyId);
            }
            return result;

            //Company result = _context.Companies.Find(companyId);
        }

        public async Task<int> GetCompanyIdByName(string companyName)
        {
            int? result = -1;
            if (!string.IsNullOrEmpty(companyName))
            {
                result = (await _context.Companies.FirstOrDefaultAsync(c => c.Name == companyName))?.Id;
                if (result.Value >= 0)
                {
                    return result.Value;
                }
            }
            return result.Value;
        }

        // CRUD: Delete
        public async Task DeleteCompanyAsync(int companyId)
        {
            try
            {
                Company company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
                if (company != null) {
                    _context.Companies.Remove(company);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
