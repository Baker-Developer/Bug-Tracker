using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extentions;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BugTrackerUser> _userManager;
        private readonly IBugTrackerProjectService _projectService;
        private readonly IBugTrackerLookupService _lookupService;
        private readonly IBugTrackerTicketService _ticketService;
        private readonly IBugTrackerFileService _fileService;
        private readonly IBugTrackerHistoryService _historyService;

        public TicketsController(UserManager<BugTrackerUser> userManager, IBugTrackerProjectService projectService, IBugTrackerLookupService lookupService, IBugTrackerTicketService ticketService, IBugTrackerFileService fileService, IBugTrackerHistoryService historyService)
        {
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
            _historyService = historyService;
        }


        public async Task<IActionResult> MyTickets()
        {
            BugTrackerUser bugTrackerUser = await _userManager.GetUserAsync(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(bugTrackerUser.Id, bugTrackerUser.CompanyId);

            return View(tickets);
        }

        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            if (User.IsInRole(nameof(Roles.Developer)) || (User.IsInRole(nameof(Roles.Submitter))))
            {
                return View(tickets.Where(t => t.Archived == false));
            }
            else
            {
                return View(tickets);
            }

        }

        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }



        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            string bugTrackerUserId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> projectManagerTickets = new();

                foreach (Ticket ticket in tickets)
                {
                    if (await _projectService.IsAssignedProjectMangerAsync(bugTrackerUserId, ticket.ProjectId))
                    {
                        projectManagerTickets.Add(ticket);
                    }
                }

                return View(projectManagerTickets);
            }


        }


        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id);
            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(Roles.Developer)),
                                                                 "Id", "FullName");

            return View(model);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId != null)
            {
                BugTrackerUser bugTrackerUser = await _userManager.GetUserAsync(User);

                // oldTicket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
                }
                catch (Exception)
                {
                    throw;
                }

                // newTicket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, bugTrackerUser.Id);

                return RedirectToAction(nameof(Details), new { id = model.Ticket.Id });
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket.Id });
        }



        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);


            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BugTrackerUser bugTrackerUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(bugTrackerUser.Id), "Id", "Name");
            }


            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {

            BugTrackerUser bugTrackerUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {

                try
                {
                    ticket.Created = DateTimeOffset.Now;
                    ticket.OwnerUserId = bugTrackerUser.Id;

                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(BugTrackerTicketStatus.New))).Value;

                    await _ticketService.AddNewTicketAsync(ticket);

                    //TODO: Ticket History
                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null, newTicket, bugTrackerUser.Id);

                    //TODO: Ticket Notification
                    return View(ticket);
                }
                catch (Exception)
                {
                    throw;
                }
            }


            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(bugTrackerUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(bugTrackerUser.Id), "Id", "Name");
            }


            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                BugTrackerUser bugTrackerUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //Add Ticket History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, bugTrackerUser.Id);

                return RedirectToAction("Edit");
            }


            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTimeOffset.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    // Add Ticket Comment History
                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);

                }
                catch (Exception)
                {
                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.UserId = _userManager.GetUserId(User);

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    // Add History For Ticket Attachment          
                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);

                }
                catch (Exception)
                {

                    throw;
                }

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }




        // GET: Tickets/Archive/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);


            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);


            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // POST: Tickets/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("AllTickets");
        }



        // POST: Tickets/Archived/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivedConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("AllTickets");
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }

    }
}
