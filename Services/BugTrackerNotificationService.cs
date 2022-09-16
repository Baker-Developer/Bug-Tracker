using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BugTrackerNotificationService : IBugTrackerNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IBugTrackerRolesService _rolesService;

        public BugTrackerNotificationService(ApplicationDbContext context,
                                                IEmailSender emailSender,
                                                IBugTrackerRolesService rolesService)
        {
            _context = context;
            _emailSender = emailSender;
            _rolesService = rolesService;
        }

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
            BugTrackerUser bugTrackerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);
            if (bugTrackerUser != null)
            {
                string bugTrackerUserEmail = bugTrackerUser.Email;
                string message = notification.Message;

                // Send Email
                try
                {
                    await _emailSender.SendEmailAsync(bugTrackerUserEmail, emailSubject, message);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            try
            {
                List<BugTrackerUser> members = await _rolesService.GetUsersInRoleAsync(role, companyId);

                foreach (BugTrackerUser bugTrackerUser in members)
                {
                    notification.RecipientId = bugTrackerUser.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<BugTrackerUser> members)
        {
            try
            {
                foreach (BugTrackerUser bugTrackerUser in members)
                {
                    notification.RecipientId = bugTrackerUser.Id;
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
