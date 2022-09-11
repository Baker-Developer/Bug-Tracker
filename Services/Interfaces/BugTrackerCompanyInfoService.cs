using BugTracker.Data;
using BugTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public class BugTrackerCompanyInfoService : IBugTrackerInfoService
    {
        private readonly ApplicationDbContext _context;

        public BugTrackerCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BugTrackerUser>> GetAllMembersAsync(int companyId)
        {
            List<BugTrackerUser> result = new();
            result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
            return result;
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            List<Project> result = new();
            result = await _context.Projects.Where(p => p.CompanyId == companyId)
                .Include(p => p.Members)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.Comments)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.TicketStatus)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.TicketPriority)
                 .Include(p => p.Tickets)
                    .ThenInclude(t => t.TicketType)
                .Include(p => p.ProjectPriority)
                .ToListAsync();
            return result;
        }

        public Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            throw new System.NotImplementedException();
        }
    }
}
