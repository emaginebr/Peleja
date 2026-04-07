namespace Peleja.Infra.Interfaces.Repositories;

public interface IPageRepository<TModel>
{
    Task<TModel?> GetByUrlAndSiteIdAsync(long siteId, string pageUrl);
    Task<TModel?> GetByIdAndSiteIdAsync(long pageId, long siteId);
    Task<List<(TModel Page, int CommentCount)>> GetBySiteIdWithCommentsAsync(long siteId, long? cursor, int pageSize);
    Task<TModel> CreateAsync(TModel page);
}
