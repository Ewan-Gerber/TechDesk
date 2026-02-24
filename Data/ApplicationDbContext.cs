using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechDesk.Models;

namespace TechDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           
            // Seed default values:
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Software", Description = "Application or OS issues" },
                new Category { Id = 2, Name = "Hardware", Description = "Physical devices issues" },
                new Category { Id = 3, Name = "Network", Description = "Connectivity and network issues" },
                new Category { Id = 4, Name = "Access & Permissions", Description = "Login and access requests" },
                new Category { Id = 5, Name = "Other", Description = "General or uncategorised issues" }
            );
        }
    }
}
