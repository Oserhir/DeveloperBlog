using TheBlogProject.Models;

namespace TheBlogProject.Services.Interfaces
{
    public interface ICommentService
    {
        public Task<List<Comment>> GetAllCommentAsync();
        public Task<List<Comment>> GetModeratedComments();
        public Task<Comment> GetCommentByIdAsync(int CommentId);
        public Task AddNewCommentAsync(Comment comment);
        public Task UpdateCommentAsync(Comment comment);
        public Task RemoveCommentAsync(Comment comment);
    }
}
