using DUST.Data;
using DUST.Models;
using DUST.Models.Enums;
using DUST.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRolesService _rolesService;
        private readonly IProjectService _projectService;

        public TicketService(ApplicationDbContext context, IRolesService rolesService,
            IProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        // CRUD Add
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        // CRUD Archive (delete)
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

            try
            {
                if (ticket != null)
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;
                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
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
                Debug.WriteLine($"*** ERROR *** - Error Assigning the ticket. ---> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                        .Include(t => t.DeveloperUser)
                                                        .Include(t => t.OwnerUser)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Notifications)
                                                        .Include(t => t.History)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            // Because the return type of this method can be a nullable int, .Value must be used
            // to safely stored the value into a variable of type int.
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Notifications)
                                                        .Include(t => t.History)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                     .Where(t => t.TicketPriorityId == priorityId)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            // Because the return type of this method can be a nullable int, .Value must be used
            // to safely stored the value into a variable of type int.
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Notifications)
                                                        .Include(t => t.History)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                     .Where(t => t.TicketPriorityId == statusId)
                                                     .ToListAsync();
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            // Because the return type of this method can be a nullable int, .Value must be used
            // to safely stored the value into a variable of type int.
            int typeId = (await LookupTicketStatusIdAsync(typeName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Notifications)
                                                        .Include(t => t.History)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                     .Where(t => t.TicketPriorityId == typeId)
                                                     .ToListAsync();
                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.Archived == true).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }           
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }                          
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        // CRUD Get
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await _context.Tickets
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.OwnerUser)
                                              .Include(t => t.Project)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.TicketStatus)
                                              .FirstOrDefaultAsync(t => t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<DUSTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            DUSTUser developer = new();
            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);

                if (ticket?.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }

                return developer;               
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                if (role == RolesEnum.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);

                } 
                else if ( role == RolesEnum.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == RolesEnum.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();

                }
                else if (role == RolesEnum.ProjectManager.ToString())
                {
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = new();
            DUSTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            try
            {
                if (await _rolesService.IsUserInRoleAsync(user,RolesEnum.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllActiveProjectsByCompanyAsync(companyId)).SelectMany(p => p.Tickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RolesEnum.Developer.ToString()))
                {
                    tickets = (await _projectService.GetAllActiveProjectsByCompanyAsync(companyId)).SelectMany(p => p.Tickets)
                                                                                             .Where(t => t.DeveloperUserId == userId)
                                                                                             .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RolesEnum.Submitter.ToString()))
                {
                    tickets = (await _projectService.GetAllActiveProjectsByCompanyAsync(companyId)).SelectMany(p => p.Tickets)
                                                                                             .Where(t => t.OwnerUserId == userId)
                                                                                             .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RolesEnum.ProjectManager.ToString()))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(p => p.Tickets).ToList();
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(tp => tp.Name == priorityName);
                return priority?.Id;
            } 
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(ts => ts.Name == statusName);
                return status?.Id;
            }
            catch (Exception)
            {
                throw;
            };
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(t => t.Name == typeName);
                return type?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // CRUD Update
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
