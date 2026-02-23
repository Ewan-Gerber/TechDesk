using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechDesk.Data;
using TechDesk.Models;
using TechDesk.ViewModels;

namespace TechDesk.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentuser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Ticket> ticketsQuery;

            if (isAdmin)
            {
                ticketsQuery = _context.Tickets
                    .Include(t => t.User)
                    .Include(t => t.Category);
            }
            else
            {
                ticketsQuery = _context.Tickets
                    .Where(t => t.UserId == currentuser!.Id)
                    .Include(t => t.User)
                    .Include(t => t.Category);
            }

            var tickets = await ticketsQuery.ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
                RecentTickets = tickets.OrderByDescending(t => t.CreatedAt).Take(10).ToList()
            };

            return View(viewModel);
        }
    }
}
