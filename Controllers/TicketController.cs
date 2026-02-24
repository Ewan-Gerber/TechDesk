using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechDesk.Data;
using TechDesk.Models;
using TechDesk.ViewModels;

namespace TechDesk.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // Shows the Users current tickets
        public async Task<IActionResult> MyTickets()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var tickets = await _context.Tickets
                .Where(t => t.UserId == currentUser.Id)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        // Shows the form to create a new ticket
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateTicketViewModel
            {
                Categories = await GetCategoriesAsync()
            };
            return View(viewModel);
        }

        // Handles the form submission to create a new ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    var ticket = new Ticket
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Priority = model.Priority,
                        CategoryId = model.CategoryId,
                        UserId = currentUser!.Id,
                        Status = TicketStatus.Open,
                        CreatedAt = DateTime.Now
                    };

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ticket submitted successfully!";
                    return RedirectToAction("MyTickets");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "Inner: " + ex.InnerException?.Message);
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        ModelState.AddModelError(string.Empty, state.Key + ": " + error.ErrorMessage);
                    }
                }
            }

            model.Categories = await GetCategoriesAsync();
            return View(model);
        }

        // Shows the details of a single ticket
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.TimeEntries)
                    .ThenInclude(te => te.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Stop normal users from viewing other people's tickets
            if (!isAdmin && ticket.UserId != currentUser!.Id)
                return Forbid();

            var viewModel = new TicketDetailsViewModel
            {
                Ticket = ticket,
                IsAdmin = isAdmin,
                IsOwner = ticket.UserId == currentUser!.Id
            };

            return View(viewModel);
        }

        // Allows admins to add a comment to a ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = currentUser!.Id,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(comment);

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment added";
            return RedirectToAction("Details", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            // Only the ticket owner can mark as completed
            if (ticket.UserId != currentUser!.Id && !User.IsInRole("Admin"))
                return Forbid();

            // Can only complete a ticket that is Open or in Progress
            if (ticket.Status != TicketStatus.Open && ticket.Status != TicketStatus.InProgress)
            {
                ticket.Status = TicketStatus.Completed;
                ticket.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket marked as completed!";
            }

            return RedirectToAction("Details", new { id });
        }

        // Allows admin to fully close a completed ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (ticket.UserId != currentUser!.Id && !User.IsInRole("Admin"))
                return Forbid();

            ticket.Status = TicketStatus.Closed;
            ticket.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket closed successfully!";
            return RedirectToAction("MyTickets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            if (ticket.UserId != currentUser!.Id && !User.IsInRole("Admin"))
                return Forbid();

            ticket.Status = TicketStatus.Open;
            ticket.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket reopened successfully!";
            return RedirectToAction("MyTickets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTimer(int ticketId, string startDate, string startTime, string endDate, string endTime)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return NotFound();

            //Combine the date and time strings into full DateTime values
            if (DateTime.TryParse($"{startDate} {startTime}", out DateTime start) &&
                DateTime.TryParse($"{endDate} {endTime}", out DateTime end))
            {
                if (end <= start)
                {
                    TempData["Error"] = "End time must be after start time.";
                    return RedirectToAction("Details", new { id = ticketId });
                }

                
                ticket.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Timer set successfully!";
            }
            else
            {
                TempData["Error"] = "Invalid date or time format.";
            }

            return RedirectToAction("Details", new { id = ticketId });

        }

        // Saves a manual time entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTimeEntry(int ticketId,
            string startDate, string startTime,
            string endDate, string endTime,
            string? note)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Only ticket owner or admin can add time entries
            if (!User.IsInRole("Admin") && ticket.UserId != currentUser!.Id)
                return Forbid();

            if (DateTime.TryParse($"{startDate} {startTime}", out DateTime start) &&
                DateTime.TryParse($"{endDate} {endTime}", out DateTime end))
            {
                if (end <= start)
                {
                    TempData["Error"] = "End time must be after start time.";
                    return RedirectToAction("Details", new { id = ticketId });
                }

                // Calculate duration in minutes
                var duration = (int)(end - start).TotalMinutes;

                var entry = new TimeEntry
                {
                    TicketId = ticketId,
                    StartTime = start,
                    EndTime = end,
                    DurationMinutes = duration,
                    Note = note,
                    isManualEntry = true,
                    UserId = currentUser!.Id,
                    CreatedAt = DateTime.Now
                };

                _context.TimeEntries.Add(entry);
                ticket.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Time entry added successfully!";
            }
            else
            {
                TempData["Error"] = "Invalid date or time format.";
            }

            return RedirectToAction("Details", new { id = ticketId });
        }

        // Saves a live timer entry when the tech presses Stop
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLiveTimer(int ticketId,
            string startTime, string endTime, string? note)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Only the ticket owner can use the live timer
            if (ticket.UserId != currentUser!.Id)
                return Forbid();

            if (DateTime.TryParse(startTime, out DateTime start) &&
                DateTime.TryParse(endTime, out DateTime end))
            {
                var duration = (int)(end - start).TotalMinutes;

                var entry = new TimeEntry
                {
                    TicketId = ticketId,
                    StartTime = start,
                    EndTime = end,
                    DurationMinutes = duration,
                    Note = note,
                    isManualEntry = false,
                    UserId = currentUser!.Id,
                    CreatedAt = DateTime.Now
                };

                _context.TimeEntries.Add(entry);
                ticket.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Time entry saved!";
            }

            return RedirectToAction("Details", new { id = ticketId });
        }

        // Deletes a time entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimeEntry(int entryId, int ticketId)
        {
            var entry = await _context.TimeEntries.FindAsync(entryId);
            if (entry == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Only the person who created it or an admin can delete it
            if (entry.UserId != currentUser!.Id && !User.IsInRole("Admin"))
                return Forbid();

            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Time entry deleted.";
            return RedirectToAction("Details", new { id = ticketId });
        }

        // Private helper method to get categories for the dropdowns
        private async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}
