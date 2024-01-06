using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNewCommentAsync(Comment comment)
        {
            _context.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetAllCommentAsync()
        {
            return await _context.Comments
                .Include(p => p.BlogUser)
                .Include(p => p.Post)

                .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int CommentId)
        {
            Comment comment = await _context.Comments
                    .Include(c => c.Post)
                    //.Include(c => c.Moderated)
                    .Include(c => c.BlogUser)
                    .FirstOrDefaultAsync(m => m.Id == CommentId);

            return comment;
        }

        public async Task<List<Comment>> GetModeratedComments()
        {
            return await _context.Comments.Where(m => m.ModeratorId != null).ToListAsync();
        }

        public async Task RemoveCommentAsync(Comment comment)
        {
            _context.Comments.Remove(comment);
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Update(comment);
            await _context.SaveChangesAsync();
        }
    }
}
