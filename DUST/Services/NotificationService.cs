using DUST.Data;
using DUST.Models;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IRolesService _rolesService;

        public NotificationService(ApplicationDbContext context, IEmailSender emailSender, IRolesService rolesService)
        {
            _context = context;
            _emailSender = emailSender;
            _rolesService = rolesService;
        }

        /// <summary>
        /// Adds a notification to the Notifications table in the database
        /// </summary>
        /// <param name="notification">The notification to add</param>
        /// <returns></returns>
        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                                                                 .Include(n => n.Recipient)
                                                                 .Include(n => n.Sender)
                                                                 .Include(n => n.Ticket)
                                                                    .ThenInclude(t => t.Project)
                                                                .Where(n => n.RecipientId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                                                                 .Include(n => n.Recipient)
                                                                 .Include(n => n.Sender)
                                                                 .Include(n => n.Ticket)
                                                                    .ThenInclude(t => t.Project)
                                                                .Where(n => n.SenderId == userId).ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            bool isEmailSent = false;
            // First make sure that the recipient exists
            DUSTUser recipient = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);
            if (recipient != null)
            {
                try
                {
                    await _emailSender.SendEmailAsync(recipient.Email, emailSubject, notification.Message);
                    isEmailSent = true;
                    return isEmailSent;
                }
                catch (Exception)
                {

                    throw;
                }                
            }
            else
            {
                return isEmailSent;
            }
       
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            try
            {
                List<DUSTUser> recipients = await _rolesService.GetUsersInRoleAsync(role, companyId);
                foreach (DUSTUser recipient in recipients)
                {
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<DUSTUser> members)
        {
            try
            {
                foreach (DUSTUser member in members)
                {
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
