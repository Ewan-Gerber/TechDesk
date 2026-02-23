using Microsoft.AspNetCore.Identity;
using TechDesk.Models;

namespace TechDesk.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define roles
            string[] roles = { "Admin", "User" };
            // Create roles if they don't exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Admin 1:
            await CreateAdminUser(userManager,
                email: "admin1@techdesk.com",
                firstName: "Admin",
                lastName: "One",
                department: "IT",
                password: "Admin@123");

            await CreateAdminUser(userManager,
                email: "admin2@techdesk.com",
                firstName: "Admin",
                lastName: "Two",
                department: "Software",
                password: "Admin@!@#");
        }

        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager, string email, string firstName, string lastName, string department, string password)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Department = department
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
