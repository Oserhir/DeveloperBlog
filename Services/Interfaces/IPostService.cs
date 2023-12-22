using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public interface IPostService
    {
        public Task<List<Post>> GetAllPostAsync();
        public Task<Post> GetPostByIdAsync(int PostId);
        public Task AddNewPostAsync(Post post);
        public Task UpdatePostAsync(Post post);
        public Task RemovePostAsync(Post post);
        
    }
}
