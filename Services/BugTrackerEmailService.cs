using BugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BugTrackerEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public BugTrackerEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
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
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

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
