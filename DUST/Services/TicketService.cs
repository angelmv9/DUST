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
            _context.Add(ticket);
            await _context.SaveChangesAsync();
        }

        // CRUD Archive (delete)
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            ticket.Archived = true;
            await UpdateTicketAsync(ticket);
        }

        public Task AssignTicketAsync(int ticketId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
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

        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            throw new NotImplementedException();
        }

        // CRUD Get
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            return ticket;
        }

        public Task<DUSTUser> GetTicketDeveloperAsync(int ticketId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            throw new NotImplementedException();
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
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
