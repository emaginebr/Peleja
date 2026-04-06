namespace Peleja.Infra.Interfaces.Repositories;

public interface ICommentLikeRepository<TModel>
{
    Task<TModel?> GetAsync(long commentId, long userId);
    Task<TModel> CreateAsync(TModel commentLike);
    Task DeleteAsync(TModel commentLike);
    Task<bool> ExistsAsync(long commentId, long userId);
}
