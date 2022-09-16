using BugTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBugTrackerInfoService
    {
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);
        public Task<List<BugTrackerUser>> GetAllMembersAsync(int companyId);
        public Task<List<Project>> GetAllProjectsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);

    }
}
