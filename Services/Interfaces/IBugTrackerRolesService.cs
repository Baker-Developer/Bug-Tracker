using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBugTrackerRolesService
    {
        public Task<bool> IsUserInRoleAsync(BugTrackerUser user, string roleName);
        public Task<IEnumerable<string>> GetUserRolesAsync(BugTrackerUser user);
        public Task<List<IdentityRole>> GetRolesAsync();
        public Task<bool> AddUserToRoleAsync(BugTrackerUser user, string roleName);
        public Task<bool> RemoveUserFromRoleAsync(BugTrackerUser user, string roleName);
        public Task<bool> RemoveUserFromRolesAsync(BugTrackerUser user, IEnumerable<string> roles);
        public Task<List<BugTrackerUser>> GetUsersInRoleAsync(string roleName, int companyId);
        public Task<List<BugTrackerUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        public Task<string> GetRoleNameByIdAsync(string roleId);
    }
}
