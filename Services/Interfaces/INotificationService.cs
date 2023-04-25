using DUST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services.Interfaces
{
    public interface INotificationService
    {
        public Task AddNotificationAsync(Notification notificaiton);
        public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);
        public Task<List<Notification>> GetSentNotificationsAsync(string userId);
        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);
        public Task SendMembersEmailNotificationsAsync(Notification notification, List<DUSTUser> members);
        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);

    }
}
