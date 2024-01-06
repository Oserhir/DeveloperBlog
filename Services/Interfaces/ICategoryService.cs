using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task AddNewCategoryAsync(Category category);
        public Task<List<Category>> GetAllCategoriesAsync();
        public Task<Category> GetCategoryByIdAsync(int CategoryId);
    }
}
