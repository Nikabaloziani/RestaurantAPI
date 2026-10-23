
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantDATA.Entities;

namespace RestaurantDATA.Roles
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            Console.WriteLine("[SEED] Starting database seeding...");

            // 1. Create roles
            string[] roles = { "Admin", "Waiter", "Customer" };

            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                    continue;

                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role {role}: " +
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }

                Console.WriteLine($"[SEED] Role created: {role}");
            }

            // 2. Read bootstrap credentials
            var adminEmail = Environment.GetEnvironmentVariable(
                "BOOTSTRAP_ADMIN_EMAIL")?.Trim();

            var adminPassword = Environment.GetEnvironmentVariable(
                "BOOTSTRAP_ADMIN_PASSWORD");

            bool isDevelopment =
                string.Equals(
                    Environment.GetEnvironmentVariable(
                        "ASPNETCORE_ENVIRONMENT"),
                    "Development",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    Environment.GetEnvironmentVariable(
                        "DOTNET_ENVIRONMENT"),
                    "Development",
                    StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(adminEmail) &&
                string.IsNullOrWhiteSpace(adminPassword))
            {
                Console.WriteLine(
                    "[SEED] Admin skipped: bootstrap credentials not set.");

                Console.WriteLine(
                    "[SEED] Total users: " +
                    await userManager.Users.CountAsync());

                return;
            }

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Both BOOTSTRAP_ADMIN_EMAIL and " +
                    "BOOTSTRAP_ADMIN_PASSWORD must be configured.");
            }

            if (adminPassword.Length < 12)
            {
                throw new InvalidOperationException(
                    "Admin password must contain at least 12 characters.");
            }

            // 3. Find or create Admin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                Console.WriteLine("[SEED] Creating Admin account...");

                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Restaurant",
                    LastName = "Administrator"
                };

                var created = await userManager.CreateAsync(
                    adminUser, adminPassword);

                if (!created.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Admin creation failed: " +
                        string.Join("; ",
                            created.Errors.Select(e => e.Description)));
                }

                var assigned = await userManager.AddToRoleAsync(
                    adminUser, "Admin");

                if (!assigned.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Admin role assignment failed: " +
                        string.Join("; ",
                            assigned.Errors.Select(e => e.Description)));
                }

                Console.WriteLine("[SEED] Admin created successfully.");
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    throw new InvalidOperationException(
                        "Bootstrap email belongs to a non-admin account.");
                }

                Console.WriteLine("[SEED] Existing Admin found.");

                if (isDevelopment)
                {
                    var resetToken =
                        await userManager.GeneratePasswordResetTokenAsync(
                            adminUser);

                    var reset = await userManager.ResetPasswordAsync(
                        adminUser,
                        resetToken,
                        adminPassword);

                    if (!reset.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Admin password reset failed: " +
                            string.Join("; ",
                                reset.Errors.Select(e => e.Description)));
                    }

                    Console.WriteLine(
                        "[SEED] Development Admin password updated.");
                }
            }

            Console.WriteLine(
                "[SEED] Total users: " +
                await userManager.Users.CountAsync());

            Console.WriteLine("[SEED] Database seeding completed.");
        }
    }
}
