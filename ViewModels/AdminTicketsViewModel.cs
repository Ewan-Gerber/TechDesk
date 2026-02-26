using TechDesk.Models;

namespace TechDesk.ViewModels
{
    public class AdminTicketsViewModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int CompletedTickets { get; set; }
        public int ClosedTickets { get; set; }

        // For the status filter dropdown
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public string? UserFilter { get; set; }
        public List<UserSelectionItem> Users { get; set; } = new List<UserSelectionItem>();
    }

    // Simple class to hold user dropdown data
    public class UserSelectionItem
    {
        public string Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
