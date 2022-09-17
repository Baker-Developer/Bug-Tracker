using BugTracker.Extentions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly IBugTrackerRolesService _rolesService;
        private readonly IBugTrackerCompanyInfoService _companyInfoService;

        public UserRolesController(IBugTrackerRolesService roleService, IBugTrackerCompanyInfoService companyInfoService)
        {
            _rolesService = roleService;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> ManagerUserRoles()
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
            
            // Return The Model To The View;
            return View(model);
        }
    }
}
