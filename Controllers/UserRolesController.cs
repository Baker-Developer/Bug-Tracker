using BugTracker.Extentions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IBugTrackerRolesService _rolesService;
        private readonly IBugTrackerCompanyInfoService _companyInfoService;

        public UserRolesController(IBugTrackerRolesService roleService, IBugTrackerCompanyInfoService companyInfoService)
        {
            _rolesService = roleService;
            _companyInfoService = companyInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // Add An Instance Of The ViewModel As A List (model)
            List<ManageUserRolesViewModel> model = new();

            // Get CompanyId
            int companyId = User.Identity.GetCompanyId().Value;

            // Get All Company Users
            List<BugTrackerUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            // Loop Over The Users To Populate The ViewModel
            // - Instatiate ViewModel
            // - use _rolesService
            // Create Multi-Selects
            foreach (BugTrackerUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BugTrackerUser = user;
                IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);
                model.Add(viewModel);
            }
            
            // Return The Model To The View
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            // Get The CompanyId
            int comapanyId = User.Identity.GetCompanyId().Value;

            // Instantiate The BugTrackerUser
            BugTrackerUser bugTrackerUser = (await _companyInfoService.GetAllMembersAsync(comapanyId)).FirstOrDefault(u => u.Id == member.BugTrackerUser.Id);


            // Get Roles Of The User
            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(bugTrackerUser);

            // Grab The Selected Role
            string userRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(userRole))
            {
                // Remove The User From The Roles
                if (await _rolesService.RemoveUserFromRolesAsync(bugTrackerUser, roles))
                {
                    // Add User To The New Role
                    await _rolesService.AddUserToRoleAsync(bugTrackerUser, userRole);
                }

            }



            // Navigate Back To The View

            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
