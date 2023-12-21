using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public interface IBlogService
    {
        public Task<Blog> GetBlogByIdAsync(int BlogId);
        public Task AddNewBlogAsync(Blog blog);
        public Task UpdateBlogAsync(Blog blog);
        public Task RemoveBlogAsync(Blog blog);
    }
}
