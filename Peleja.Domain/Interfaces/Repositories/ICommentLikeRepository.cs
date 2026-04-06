namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface ICommentLikeRepository
{
    Task<CommentLikeModel?> GetAsync(long commentId, long userId);
    Task<CommentLikeModel> CreateAsync(CommentLikeModel commentLike);
    Task DeleteAsync(CommentLikeModel commentLike);
    Task<bool> ExistsAsync(long commentId, long userId);
}
