using Domain.Models;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");
            var dbContext = services.GetRequiredService<DatabaseContext>();

            try
            {
                // Check if database is available (with timeout)
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    if (!await dbContext.Database.CanConnectAsync(cts.Token))
                    {
                        logger.LogWarning("Database is not available. Skipping seed data.");
                        return;
                    }
                }
                catch (Exception dbEx)
                {
                    logger.LogWarning(dbEx, "Cannot connect to database. Skipping seed data.");
                    return;
                }

                // Create Admin role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var adminRole = new ApplicationRole
                    {
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };
                    await roleManager.CreateAsync(adminRole);
                    logger.LogInformation("Admin role created");
                }

                // Create Admin user if it doesn't exist
                var adminUser = await userManager.FindByNameAsync("admin");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        NormalizedUserName = "ADMIN",
                        Email = "admin@ecommerce.com",
                        NormalizedEmail = "ADMIN@ECOMMERCE.COM",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin user created with username: admin, password: Admin@123");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError($"Failed to create admin user: {errors}");
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding admin user");
                // Don't throw during build/NSwag generation - just log the error
                // This allows the application to start even if database is not available
            }
        }
    }
}

