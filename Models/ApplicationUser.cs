using Microsoft.AspNetCore.Identity;

namespace TechDesk.Models
{
    public class ApplicationUser : IdentityUser
    {
        //Identity already gives us:
        // Id, Email, UserName, PasswordHash, PhoneNumber

        // Adding extra fields:

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        // This is a navigation property
        // This means one User can have many Tickets
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }   
    
}
