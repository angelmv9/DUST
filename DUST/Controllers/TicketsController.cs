using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DUST.Data;
using DUST.Models;
using Microsoft.AspNetCore.Identity;
using DUST.Extensions;
using DUST.Models.Enums;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using DUST.Models.ViewModels;

namespace DUST.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<DUSTUser> _userManager;
        private readonly IProjectService _projectService;
        private readonly ILookupService _lookupService;
        private readonly ITicketService _ticketService;
        private readonly IFilesService _filesService;

        #region Constructor
        public TicketsController(ApplicationDbContext context,
    UserManager<DUSTUser> userManager,
    IProjectService projectService,
    ILookupService lookupService,
    ITicketService ticketService, IFilesService filesService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _filesService = filesService;
        }
        #endregion

        #region Index Views
        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // Get: MyTickets
        public async Task<IActionResult> MyTickets()
        {
            DUSTUser currentUser = await _userManager.GetUserAsync(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(currentUser.Id, currentUser.CompanyId);
            return View(tickets);
        }

        // Get: AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            if (User.IsInRole(nameof(RolesEnum.Admin)) || User.IsInRole(nameof(RolesEnum.ProjectManager)))
            {
                return View(tickets);
            }
            else
            {
                return View(tickets.Where(t => t.Archived == false));
            }
        }

        // Get: ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> archivedTickets = await _ticketService.GetArchivedTicketsAsync(companyId);
            return View(archivedTickets);
        }

        //Get: UnassignedTickets
        [Authorize(Roles="Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            string currentUserId = _userManager.GetUserId(User);
            int companyId = User.Identity.GetCompanyId().Value;
            List<Ticket> unassignedTickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(RolesEnum.Admin)))
            {
                return View(unassignedTickets);
            }
            else
            {
                List<Ticket> pmTickets = new();
                foreach (Ticket ticket in unassignedTickets)
                {
                    string projectManagerId = (await _projectService.GetProjectManagerAsync(ticket.ProjectId))?.Id;

                    if (currentUserId == projectManagerId)
                    {
                        pmTickets.Add(ticket);
                    }
                }

                return View(pmTickets);
            }
        }
        #endregion
      
        #region Details
        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? ticketId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(ticketId.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        #endregion

        #region Create
        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            DUSTUser ownerUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole(nameof(RolesEnum.Admin)))
            {
                List<Project> projects = await _projectService.GetAllActiveProjectsByCompanyAsync(ownerUser.CompanyId);
                ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            }
            else
            {
                List<Project> projects = await _projectService.GetUserProjectsAsync(ownerUser.Id);
                ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            }

            List<TicketPriority> priorities = await _lookupService.GetTicketPrioritiesAsync();
            List<TicketType> types = await _lookupService.GetTicketTypesAsync();
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,TicketTypeId,TicketPriorityId,Title,Description")] Ticket ticket)
        {
            DUSTUser ownerUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = ownerUser.Id;
                int ticketStatus = (await _ticketService.LookupTicketStatusIdAsync(nameof(TicketStatusEnum.New))).Value;
                ticket.TicketStatusId = ticketStatus;
                // TODO: Add Ticket History and Notification
                await _ticketService.AddNewTicketAsync(ticket);

                return RedirectToAction(nameof(Index));
            }

            // If ModelState isn't valid

            if (User.IsInRole(nameof(RolesEnum.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllActiveProjectsByCompanyAsync(ownerUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(ownerUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View(ticket);
        }
        #endregion

        #region Edit
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

            List<TicketPriority> priorities = await _lookupService.GetTicketPrioritiesAsync();
            List<TicketStatus> statuses = await _lookupService.GetTicketStatusesAsync();
            List<TicketType> types = await _lookupService.GetTicketTypesAsync();
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(statuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId,Title,Description,Created,Updated,Archived")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                DUSTUser user = await _userManager.GetUserAsync(User);
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
                // TODO: Add Ticket History
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid

            List<TicketPriority> priorities = await _lookupService.GetTicketPrioritiesAsync();
            List<TicketStatus> statuses = await _lookupService.GetTicketStatusesAsync();
            List<TicketType> types = await _lookupService.GetTicketTypesAsync();
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(statuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }
        #endregion

        #region Add Ticket Attachment
        //POST: Tickets/AddTicketAttachment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id","TicketId","FileDescription","FormFile")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.ByteArrayData = await _filesService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileExtension = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTimeOffset.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);
                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                statusMessage = "Success: New attachment added to the Ticket";
            }
            else
            {
                statusMessage = "Error trying to upload file. Please try again";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage});
        }
        #endregion

        #region Add Ticket Comment
        //Post: Tickets/AddTicketComment/5
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
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }
        #endregion

        #region Assign Developer
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int ticketId)
        {
            AssignDeveloperViewModel model = new();
            model.Ticket = await _ticketService.GetTicketByIdAsync(ticketId);

            List<DUSTUser> developers = await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(RolesEnum.Developer));
            model.Developers = new SelectList(developers, "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DeveloperId))
            {
                await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
                return RedirectToAction(nameof(Details), new {id = model.Ticket.Id});
            }

            return RedirectToAction(nameof(AssignDeveloper), new {id = model.Ticket.Id});
        }
        #endregion

        #region Show File
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.ByteArrayData;
            string extension = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{extension}");
        }
        #endregion

        #region Archive
        // GET: Tickets/Archive/5
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

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            await _ticketService.ArchiveTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Restore
        // GET: Tickets/Restore/5
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
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }
        #endregion

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
