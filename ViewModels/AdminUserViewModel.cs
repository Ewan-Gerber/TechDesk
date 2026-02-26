namespace TechDesk.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int TicketCount { get; set; }
        public DateTime? LastTicketDate { get; set; }
    }
}
