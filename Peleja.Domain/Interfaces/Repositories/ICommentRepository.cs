namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface ICommentRepository
{
    Task<List<Comment>> GetByPageUrlAsync(long tenantId, string pageUrl, string sortBy, long? cursor, int pageSize);
    Task<Comment?> GetByIdAsync(long commentId);
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment> UpdateAsync(Comment comment);
    Task<int> GetRepliesCountAsync(long parentCommentId);
    Task<List<Comment>> GetRepliesAsync(long parentCommentId);
    Task<bool> ExistsAsync(long commentId);
}
