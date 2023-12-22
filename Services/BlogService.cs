using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task AddNewBlogAsync(Blog blog)
        {
            _context.Add(blog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Blog>> GetAllBlogsAsync()
        {
            List<Blog> blog = await _context.Blogs.ToListAsync(); 

            return blog;
        }

        public async Task<Blog> GetBlogByIdAsync(int BlogId)
        {
            Blog blog =  await _context.Blogs
                // .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(b => b.Id == BlogId);

            return blog;
        }

        public async Task RemoveBlogAsync(Blog blog)
        {
            _context.Blogs.Remove(blog);
           
        }

        public async Task UpdateBlogAsync(Blog blog)
        {
            _context.Update(blog);
            await _context.SaveChangesAsync();
        }
    }
}
