using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Roles
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = new[] { "Admin", "Waiter", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

 
            var adminEmail = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_PASSWORD");
            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            {
                if (adminPassword.Length < 12)
                    throw new InvalidOperationException("BOOTSTRAP_ADMIN_PASSWORD must have at least 12 characters.");

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "Restaurant",
                        LastName = "Administrator"
                    };
                    var created = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!created.Succeeded)
                        throw new InvalidOperationException("Admin provisioning failed due to password policy or configuration.");

                    var assigned = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!assigned.Succeeded)
                        throw new InvalidOperationException("Admin role assignment failed.");
                }
                else if (await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
   
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var reset = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
                    if (!reset.Succeeded)
                        throw new InvalidOperationException("Admin password rotation failed.");
                }
                else
                {
                    throw new InvalidOperationException("BOOTSTRAP_ADMIN_EMAIL belongs to a non-admin user.");
                }
            }
        }
    }
}