using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Extentions;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        
        private readonly IBugTrackerRolesService _rolesService;
        private readonly IBugTrackerLookupService _lookupService;
        private readonly IBugTrackerFileService _fileService;
        private readonly IBugTrackerProjectService _projectService;
        private readonly UserManager<BugTrackerUser> _userManager;
        private readonly IBugTrackerCompanyInfoService _companyInfoService;


        public ProjectsController(IBugTrackerRolesService roleService, IBugTrackerLookupService lookupService, IBugTrackerFileService fileService, IBugTrackerProjectService projectService, UserManager<BugTrackerUser> userManager, IBugTrackerCompanyInfoService companyInfoService)
        {
            _rolesService = roleService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }



        // GET: MyProjects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        // GET: AllProjects
        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompany(companyId);
            }

            return View(projects);
        }




        // GET: ArchivedProjects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = new();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignProjectManager(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignProjectManagerViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
            model.ProjectManagerList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }



        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignProjectManagerViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ProjectManagerId))
            {
                await _projectService.AddProjectManagerAsync(model.ProjectManagerId, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }
            return RedirectToAction(nameof(AssignProjectManager), new { projectId = model.Project.Id });
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BugTrackerUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
            List<BugTrackerUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

            List<BugTrackerUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                                                      .Select(m => m.Id).ToList();
                // Remove Current Members
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                //Add Selected Members
                foreach (string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                //Goto Project Details
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }


            return RedirectToAction(nameof(AssignMembers), new { id = model.Project.Id });

        }



        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);



            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            // Add ViewModel Instance AddProjectWithProjectManagerViewModel
            AddProjectWithProjectManagerViewModel model = new();
            // Load SelectList With Data ProjectManagerList & PriorityList
            model.ProjectManagerList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithProjectManagerViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }
                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    // Add Project Manager If One Was Chosen
                    if (!string.IsNullOrEmpty(model.ProjectManagerId))
                    {
                        await _projectService.AddProjectManagerAsync(model.ProjectManagerId, model.Project.Id);
                    }

                }
                catch (Exception)
                {
                    throw;
                }
                //TODO: Redirect To All Projects
                return RedirectToAction("Create");
            }

            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            // Add ViewModel Instance AddProjectWithProjectManagerViewModel
            AddProjectWithProjectManagerViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            // Load SelectList With Data ProjectManagerList & PriorityList
            model.ProjectManagerList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithProjectManagerViewModel model)
        {
            if (model != null)
            {

                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }


                    await _projectService.UpdateProjectAsync(model.Project);

                    // Add Project Manager If One Was Chosen
                    if (!string.IsNullOrEmpty(model.ProjectManagerId))
                    {
                        await _projectService.AddProjectManagerAsync(model.ProjectManagerId, model.Project.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //TODO: Redirect To All Projects
                return RedirectToAction("AllProjects");
            }

            return RedirectToAction("Edit");
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction("AllProjects");
        }


        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction("AllProjects");
        }



        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompany(companyId)).Any(p => p.Id == id);
        }
    }
}
