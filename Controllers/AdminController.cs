using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechDesk.Data;
using TechDesk.Models;
using TechDesk.ViewModels;

namespace TechDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Shows all tickets with optional filtering
        public async Task<IActionResult> AllTickets(string? statusFilter, string? priorityFilter,  string? userFilter)
        {
            // Start with all tickets
            var query = _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Category)
                .AsQueryable();

            // Apply status filter if one was selected
            if (!string.IsNullOrEmpty(statusFilter) &&
                Enum.TryParse<TicketStatus>(statusFilter, out var status))
            {
                query = query.Where(t => t.Status == status);
            }

            // Apply priority filter if one was selected
            if (!string.IsNullOrEmpty(priorityFilter) &&
                Enum.TryParse<Priority>(priorityFilter, out var priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            // Apply user filter
            if (!string.IsNullOrEmpty(userFilter))
            {
                query = query.Where(t => t.UserId == userFilter);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Get ALL tickets for the stat counts regardless of filter
            var allTickets = await _context.Tickets.ToListAsync();

            // Build the users dropdown from everyone who has at least one ticket
            var usersWithTickets = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.User != null)
                .Select(t => new UserSelectionItem
                {
                    Id = t.UserId!,
                    FullName = t.User!.FirstName + " " + t.User.LastName
                })
                .Distinct()
                .ToListAsync();

            // Remove duplicates by Id
            var uniqueUsers = usersWithTickets
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .OrderBy(u => u.FullName)
                .ToList();

            var viewModel = new AdminTicketsViewModel
            {
                Tickets = tickets,
                TotalTickets = allTickets.Count,
                OpenTickets = allTickets.Count(t => t.Status == TicketStatus.Open),
                InProgressTickets = allTickets.Count(t => t.Status == TicketStatus.InProgress),
                CompletedTickets = allTickets.Count(t => t.Status == TicketStatus.Completed),
                ClosedTickets = allTickets.Count(t => t.Status == TicketStatus.Closed),
                StatusFilter = statusFilter,
                PriorityFilter = priorityFilter,
                UserFilter = userFilter,
                Users = uniqueUsers
            };

            return View(viewModel);
        }

        // Changes the status of a ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ticketId, string status)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("AllTickets");
            }

            if (Enum.TryParse<TicketStatus>(status, out var newStatus))
            {
                ticket.Status = newStatus;
                ticket.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket status updated successfully.";
            }

            return RedirectToAction("AllTickets");
        }

        // Shows all users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                // Get ticket count for each user
                var ticketCount = await _context.Tickets
                    .CountAsync(t => t.UserId == user.Id);

                // Get their most recent ticket date
                var lastTicket = await _context.Tickets
                    .Where(t => t.UserId == user.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                userViewModels.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Department = user.Department,
                    IsAdmin = isAdmin,
                    TicketCount = ticketCount,
                    LastTicketDate = lastTicket?.CreatedAt
                });
            }

            return View(userViewModels);
        }

        // Promotes a user to Admin or demotes them to User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Prevent admins from removing their own admin role
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser!.Id)
            {
                TempData["Error"] = "You cannot change your own admin status.";
                return RedirectToAction("Users");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
                TempData["Success"] = $"{user.FirstName} has been demoted to User.";
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["Success"] = $"{user.FirstName} has been promoted to Admin.";
            }

            return RedirectToAction("Users");
        }

        // Deletes a user and all their tickets
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Prevent admins from deleting themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser!.Id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            // Delete all their tickets first
            var userTickets = await _context.Tickets
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _context.Tickets.RemoveRange(userTickets);
            await _context.SaveChangesAsync();

            // Now delete the user
            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"{user.FirstName} {user.LastName} has been deleted.";

            return RedirectToAction("Users");
        }

        // Admin can update ticket status from the details page too
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, string newStatus)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("AllTickets");
            }

            if (Enum.TryParse<TicketStatus>(newStatus, out var parsedStatus))
            {
                ticket.Status = parsedStatus;
                ticket.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Status updated to " + parsedStatus.ToString();
            }

            return RedirectToAction("Details", "Ticket", new { id = ticketId });
        }
    }
}
