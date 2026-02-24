using TechDesk.Models;

namespace TechDesk.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; } = null!;
        public string NewComment { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsOwner { get; set; }
    }
}
