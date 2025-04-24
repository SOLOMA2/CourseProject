using CourseProject.DataUser;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourseProject.Services
{
    public class SeedServices
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<AppUserDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var logger = services.GetRequiredService<ILogger<SeedServices>>();

            try
            {
                await context.Database.MigrateAsync();

                var adminSection = configuration.GetSection("Data:AdminUser");
                var adminName = adminSection["Name"];
                var adminEmail = adminSection["Email"];
                var adminPassword = adminSection["Password"];
                var adminRole = adminSection["Role"];

                if (string.IsNullOrEmpty(adminPassword))
                    throw new ArgumentNullException("Admin password not configured");

                await CreateRoleAsync(roleManager, adminRole);
                await CreateRoleAsync(roleManager, "User");

                await CreateAdminUserAsync(userManager, adminName, adminEmail, adminPassword, adminRole, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errrrrooor");
                throw;
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task CreateAdminUserAsync(UserManager<AppUser> userManager, string userName, string email, string password,string role, ILogger logger)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                logger.LogInformation("Create admin: {Email}", email);

                var adminUser = new AppUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, role);
                }
            }
        }
    }
}