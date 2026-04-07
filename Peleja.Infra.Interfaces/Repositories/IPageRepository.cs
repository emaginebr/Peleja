namespace Peleja.Infra.Interfaces.Repositories;

public interface IPageRepository<TModel>
{
    Task<TModel?> GetByUrlAndSiteIdAsync(long siteId, string pageUrl);
    Task<TModel> CreateAsync(TModel page);
}
