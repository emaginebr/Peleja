namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface ICommentRepository
{
    Task<List<CommentModel>> GetByPageIdAsync(long pageId, string sortBy, long? cursor, int pageSize);
    Task<CommentModel?> GetByIdAsync(long commentId);
    Task<CommentModel> CreateAsync(CommentModel comment);
    Task<CommentModel> UpdateAsync(CommentModel comment);
    Task<int> GetRepliesCountAsync(long parentCommentId);
    Task<List<CommentModel>> GetRepliesAsync(long parentCommentId);
    Task<bool> ExistsAsync(long commentId);
}
