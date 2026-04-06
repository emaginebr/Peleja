namespace Peleja.Infra.Interfaces.Repositories;

public interface ICommentRepository<TModel>
{
    Task<List<TModel>> GetByPageIdAsync(long pageId, string sortBy, long? cursor, int pageSize);
    Task<TModel?> GetByIdAsync(long commentId);
    Task<TModel> CreateAsync(TModel comment);
    Task<TModel> UpdateAsync(TModel comment);
    Task<int> GetRepliesCountAsync(long parentCommentId);
    Task<List<TModel>> GetRepliesAsync(long parentCommentId);
    Task<bool> ExistsAsync(long commentId);
}
