using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Services
{
    public class PostService : IPostService
    {

        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNewPostAsync(Post post)
        {
            _context.Add(post);
            await _context.SaveChangesAsync();
        }

        public Task<List<Post>> GetAllPostAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Post> GetPostByIdAsync(int PostId)
        {
            Post post = await _context.Posts

                .Include(p => p.Blog )
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Include(p => p.BlogUser)

                .FirstOrDefaultAsync(p => p.Id == PostId);

            return post;
        }

        public async Task RemovePostAsync(Post post)
        {
            _context.Posts.Remove(post);
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
