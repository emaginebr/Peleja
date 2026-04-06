namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface ICommentLikeRepository
{
    Task<CommentLike?> GetAsync(long commentId, long userId);
    Task<CommentLike> CreateAsync(CommentLike commentLike);
    Task DeleteAsync(CommentLike commentLike);
    Task<bool> ExistsAsync(long commentId, long userId);
}
