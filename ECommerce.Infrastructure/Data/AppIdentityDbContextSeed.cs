using ECommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Data
{
    public class AppIdentityDbContextSeed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new AppUser
                {
                    DisplayName = "Ahmed Hanafy",
                    Email = "ahmed@example.com",
                    UserName = "ahmed@example.com",
                    Address = new Address
                    {
                        FirstName = "Ahmed",
                        LastName = "Hanafy",
                        Street = "10 Main St",
                        City = "Cairo",
                        State = "EG",
                        ZipCode = "12345"
                    }
                };

                await userManager.CreateAsync(user, "Password123!");

                // NOTE: Roles assignment for seeded users will be added here
            }
        }
    }
}