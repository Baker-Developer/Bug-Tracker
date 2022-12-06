using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services.Interfaces;
using BugTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BugTrackerUser> _userManager;
        private readonly IBugTrackerInviteService _inviteUser;
        private readonly IBugTrackerCompanyInfoService _companyInfo;
        private readonly IBugTrackerProjectService _projectInfo;
        private readonly IEmailSender _emailSender;

        public InvitesController(ApplicationDbContext context, IBugTrackerInviteService inviteUser, UserManager<BugTrackerUser> userManager, IEmailSender emailSender, IBugTrackerProjectService projectInfo, IBugTrackerCompanyInfoService companyInfo)
        {
            _context = context;
            _inviteUser = inviteUser;
            _userManager = userManager;
            _emailSender = emailSender;
            _projectInfo = projectInfo;
            _companyInfo = companyInfo;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInviteViewModel invite)
        {

            if (ModelState.IsValid)
            {

                return View(invite);
            }

            return View(invite);
        }

        // GET: Invites/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var invite = await _context.Invites.FindAsync(id);
            //if (invite == null)
            //{
            //    return NotFound();
            //}
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View();
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            //if (id != invite.Id)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(invite);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!InviteExists(invite.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", invite.CompanyId);
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View();
        }

        // GET: Invites/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var invite = await _context.Invites
            //    .Include(i => i.Company)
            //    .Include(i => i.Invitee)
            //    .Include(i => i.Invitor)
            //    .Include(i => i.Project)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (invite == null)
            //{
            //    return NotFound();
            //}

            return View();
        }

        // POST: Invites/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var invite = await _context.Invites.FindAsync(id);
            //_context.Invites.Remove(invite);
            //await _context.SaveChangesAsync();
            return View();
        }

        private bool InviteExists(int id)
        {
            return _context.Invites.Any(e => e.Id == id);
        }
    }
}
