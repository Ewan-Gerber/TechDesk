namespace TechDesk.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Note { get; set; }
        public bool isManualEntry { get; set; }
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
