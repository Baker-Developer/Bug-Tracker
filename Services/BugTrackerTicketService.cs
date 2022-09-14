using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BugTrackerTicketService : IBugTrackerTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBugTrackerRolesService _rolesService;
        private readonly IBugTrackerProjectService _projectService;

        public BugTrackerTicketService(ApplicationDbContext context, IBugTrackerRolesService rolesService, IBugTrackerProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            _context.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            ticket.Archived = true;
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public Task AssignTicketAsync(int ticketId, string userId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                .Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.History)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
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
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.History)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                     .Where(t => t.TicketPrioityId == priorityId)
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
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.History)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                     .Where(t => t.TicketStatusId == statusId)
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
            int typeId = (await LookupTicketStatusIdAsync(typeName)).Value;
            try
            {
                List<Ticket> tickets = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.History)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                     .Where(t => t.TicketTypeId == typeId)
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
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId)).ToList().Where(t => t.Archived == true).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }


        }

        public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<BugTrackerUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BugTrackerUser developer = new();
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
                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
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
            BugTrackerUser bugTrackerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(bugTrackerUser, Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId)).SelectMany(p => p.Tickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(bugTrackerUser, Roles.Developer.ToString()))
                {
                    tickets = (await _projectService
                        .GetAllProjectsByCompany(companyId))
                           .SelectMany(p => p.Tickets).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(bugTrackerUser, Roles.Submitter.ToString()))
                {
                    tickets = (await _projectService
                        .GetAllProjectsByCompany(companyId))
                            .SelectMany(p => p.Tickets).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(bugTrackerUser, Roles.ProjectManager.ToString()))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets).ToList();
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
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(p => p.Name == priorityName);
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
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(p => p.Name == statusName);
                return status?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(p => p.Name == typeName);
                return type?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
