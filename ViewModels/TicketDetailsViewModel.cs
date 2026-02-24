using TechDesk.Models;

namespace TechDesk.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; } = null!;
        public string NewComment { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsOwner { get; set; }

        // Total minutes across all time entries
        public int TotalMinutes => Ticket?.TimeEntries?.Sum(t => t.DurationMinutes) ?? 0;

        // Formatted total like "2h 30m"
        public string TotalTimeFormatted
        {
            get
            {
                var hours = TotalMinutes / 60;
                var minutes = TotalMinutes % 60;
                if (hours == 0) return $"{minutes}m";
                if (minutes == 0) return $"{hours}h";
                return $"{hours}h {minutes}m";
            }
        }
    }
}
