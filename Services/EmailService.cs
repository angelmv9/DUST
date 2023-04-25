using DUST.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        // Configuration variables
        private readonly string mailConfigVar = Environment.GetEnvironmentVariable("MAIL_SETTINGS_EMAIL");
        private readonly string mailPassConfigVar = Environment.GetEnvironmentVariable("MAIL_SETTINGS_PASSWORD");
        private readonly string mailHostConfigVar = Environment.GetEnvironmentVariable("MAIL_SETTINGS_HOST");
        private readonly string mailPortConfigVar = Environment.GetEnvironmentVariable("MAIL_SETTINGS_PORT");

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            // Use configuration variables if present, else use default settings
            string mail = string.IsNullOrEmpty(mailConfigVar) ? _mailSettings.Mail : mailConfigVar;
            string mailHost = string.IsNullOrEmpty(mailHostConfigVar) ? _mailSettings.Host : mailHostConfigVar;
            int mailPort = string.IsNullOrEmpty(mailPortConfigVar) ? _mailSettings.Port : int.Parse(mailPortConfigVar);
            string mailPassword = string.IsNullOrEmpty(mailPassConfigVar) ? _mailSettings.Password : mailPassConfigVar;

            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(mail);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = builder.ToMessageBody();

            try
            {
                using var smtp = new SmtpClient();
                smtp.Connect(mailHost, mailPort, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(mail, mailPassword);

                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
