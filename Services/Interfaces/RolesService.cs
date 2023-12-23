using Microsoft.AspNetCore.Identity;
using TheBlogProject.Data;
using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public class RolesService : IRolesService
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public RolesService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
            return result;
        }
    }
}
