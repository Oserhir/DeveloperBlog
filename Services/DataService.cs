using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Enums;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Services
{
    public class DataService  
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<BTUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // Public Methods

        #region Manage Data Async
        public async Task ManageDataAsync()
        {
            // Create the Db if it doesn’t exist
            await _dbContext.Database.MigrateAsync();

            // 1: Seed a few roles into the system
            await SeedRolesAsync();

            // 2: Seed a few users into the system
            await SeedUsersAsync();
        }
        #endregion


        // Private Methods

        #region Seed Roles Async
        private async Task SeedRolesAsync()
        {

            // If there are already Roles in the system, do nothing.
            if (_dbContext.Roles.Any())
            {
                return;
            }

            else // Otherwise create a few roles
            {
                foreach (var role in Enum.GetNames(typeof(BlogRole)))
                {
                    // Use Role Manager to create roles
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        #endregion


        #region Seed Users Async
        private async Task SeedUsersAsync()
        {
            // If there are already Users in the system, do nothing.
            if (_dbContext.Users.Any())
            {
                return;
            }

            // Step 1: Creates a new instance of BTUser
            // Administrator
            var adminUser = new BTUser
            {
                Email = "admin@example.com",
                UserName = "adminUser",
                FirstName = "Youssef",
                LastName = "Doe",
                DisplayName = "JD_123",
                EmailConfirmed = true,
            };

            // Step 2: Use the UserManager to create a new user that is defined by the adminUser
            await _userManager.CreateAsync(adminUser, "Abc&123!");

            // Step 3: Add this new user to the Administrator role
            await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());

            // Moderator
            var modUser = new BTUser
            {
                Email = "moderator@example.com",
                UserName = "moderatorUser",
                FirstName = "Alex",
                LastName = "Smith",
                DisplayName = "AlexS_456",
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(modUser, "Abc&123!");
            await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());

        }
        #endregion

    }
}
