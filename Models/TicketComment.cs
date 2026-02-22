namespace TechDesk.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Foreign key to Ticket:
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        //Foreign key to the User who wrote the comment:
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
