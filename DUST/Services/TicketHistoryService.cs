using DUST.Data;
using DUST.Models;
using DUST.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class TicketHistoryService : ITicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketService _ticketService;

        public TicketHistoryService(ApplicationDbContext context, ITicketService ticketService)
        {
            _context = context;
            _ticketService = ticketService;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            // Ticket didn't exist before. New ticket is being added
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    UserId = userId,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    Description = "New ticket created"
                };
                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                if (oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket title: {newTicket.Title}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket description: {newTicket.Description}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {

                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "TicketPriority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {

                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "TicketStatus",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket status: {newTicket.TicketStatus.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {

                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "TicketType",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket type: {newTicket.TicketType.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {

                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        UserId = userId,
                        Property = "DeveloperUser",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTimeOffset.Now,
                        Description = $"New ticket developer: {newTicket.DeveloperUser.FullName}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");
                description = $"New {description} added to ticket: {ticket.Title}";

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    UserId = userId,
                    Property = model,
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    Description = description
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<TicketHistory> history = (await _ticketService.GetAllTicketsByCompanyAsync(companyId))
                                                 .SelectMany(t => t.History)
                                                 .ToList();
                return history;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                List<TicketHistory> history = (await _ticketService.GetAllTicketsByCompanyAsync(companyId))
                                                .Where(t => t.ProjectId == projectId)
                                                .SelectMany(t => t.History)
                                                .ToList();
                return history;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
