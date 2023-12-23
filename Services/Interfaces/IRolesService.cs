using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public interface IRolesService
    {
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);

    }
}
