namespace TechDesk.Models
{
    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed
    }

    public class Ticket
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        //Using the enums:
        public Priority Priority { get; set; } = Priority.Low;
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        //Auto set when ticket is created:
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        //Foreign Key - links this ticket to a User
        // ? is there so it can be null
        public string ? UserId { get; set; }

        // Navigation property - allows us to access the User details from a Ticket
        public ApplicationUser ? User { get; set; }

        //Foreign Key - links this ticket to a Category
        public int CategoryId { get; set; }

        //Navigation property - allows us to access the Category details from a Ticket
        public Category ? Category { get; set; }

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

    }
}
