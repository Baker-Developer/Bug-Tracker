using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BugTrackerProjectService : IBugTrackerProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBugTrackerRolesService _rolesService;

        public BugTrackerProjectService(ApplicationDbContext context, IBugTrackerRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        // CRUD - Create

        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BugTrackerUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"----ERROR---- \n ERROR REMOVING CURRENT PROJECT MANAGER. ----> {ex.Message}");
                    return false;
                }
            }
            // ADD THE NEW PROJECT MANAGER
            try
            {
                await AddUserToProjectAsync(userId, projectId);
                //await AddProjectManagerAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----ERROR---- \n ERROR ADDING NEW PROJECT MANAGER. ----> {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BugTrackerUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
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
            else
            {
                return false;
            }

        }

        // CRUD - Archive (Delete)
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<BugTrackerUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<BugTrackerUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<BugTrackerUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
            List<BugTrackerUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

            List<BugTrackerUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

            return teamMembers;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();


            projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);
            int priorityId = await LookupProjectPriorityId(priorityName);
            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                            .ToListAsync();

            return projects;
        }

        public Task<List<BugTrackerUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        // CRUD - Read
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                //Project project = await _context.Projects
                //                    .Include(p => p.Tickets)
                //                    .Include(p => p.Members)
                //                    .Include(p => p.ProjectPriority)
                //                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                Project project = await _context.Projects
                                       .Include(p => p.Tickets)
                                          .ThenInclude(t => t.TicketPriority)
                                        .Include(p => p.Tickets)
                                          .ThenInclude(t => t.TicketStatus)
                                        .Include(p => p.Tickets)
                                          .ThenInclude(t => t.TicketType)
                                        .Include(p => p.Tickets)
                                          .ThenInclude(t => t.DeveloperUser)
                                         .Include(p => p.Tickets)
                                          .ThenInclude(t => t.OwnerUser)
                                         .Include(p => p.Members)
                                         .Include(p => p.ProjectPriority)
                                         .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                return project;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<BugTrackerUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            foreach (BugTrackerUser members in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(members, Roles.ProjectManager.ToString()))
                {
                    return members;
                }
            }
            return null;
        }

        public async Task<List<BugTrackerUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            List<BugTrackerUser> members = new();

            foreach (var user in project.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, role))
                {
                    members.Add(user);
                }
            }
            return members;
        }

        public Task<List<BugTrackerUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();

            try
            {
                projects = await _context.Projects
                                            .Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId)
                                            .Where(p => p.CompanyId == companyId).ToListAsync();
                foreach (Project project in projects)
                {
                    if ((await GetProjectMembersByRoleAsync(project.Id, nameof(Roles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }


        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.Company)
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.Members)
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                            .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"----ERROR---- \n ERROR GETTING USER FROM PROJECT.  ----> {ex.Message}");
                throw;
            }

        }

        public async Task<List<BugTrackerUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BugTrackerUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();
            return users.Where(u => u.CompanyId == companyId).ToList();
        }


        public async Task<bool> IsAssignedProjectMangerAsync(string userId, int projectId)
        {
            try
            {
                string projectManagerId = (await GetProjectManagerAsync(projectId))?.Id;

                if (projectManagerId == userId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            bool result = false;
            if (project != null)
            {
                result = project.Members.Any(m => m.Id == userId);
            }
            return result;
        }

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
            return priorityId;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(p => p.Id == projectId);
            try
            {
                foreach (BugTrackerUser member in project?.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BugTrackerUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"----ERROR---- \n ERROR REMOVING USER FROM PROJECT.  ----> {ex.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BugTrackerUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BugTrackerUser bugTrackerUser in members)
                {
                    try
                    {
                        project.Members.Remove(bugTrackerUser);
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
                Console.WriteLine($"----ERROR---- \n ERROR REMOVING USER FROM PROJECT.  ----> {ex.Message}");
                throw;
            }
        }


        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        // CRUD - Update
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
