using CaseManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CaseManagementSystem.Data.Seed
{
    public static class IdentitySeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Supervisor",
                "Expert"
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));
                }
            }

            const string supervisorUserName = "supervisor";

            var supervisor =
                await userManager.FindByNameAsync(supervisorUserName);

            if (supervisor == null)
            {
                supervisor = new ApplicationUser
                {
                    UserName = supervisorUserName,
                    Email = "supervisor@cms.local",
                    FullName = "Initial Supervisor",
                    EmployeeNumber = "SUP001",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    supervisor,
                    "Supervisor@123");

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(supervisor, "Supervisor"))
            {
                await userManager.AddToRoleAsync(
                    supervisor,
                    "Supervisor");
            }
        }
    }
}