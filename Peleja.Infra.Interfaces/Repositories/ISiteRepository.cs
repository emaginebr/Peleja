namespace Peleja.Infra.Interfaces.Repositories;

public interface ISiteRepository<TModel>
{
    Task<TModel?> GetByClientIdAsync(string clientId);
    Task<TModel?> GetByUrlAsync(string siteUrl);
    Task<List<TModel>> GetByUserIdAsync(long userId);
    Task<TModel> CreateAsync(TModel site);
    Task<TModel> UpdateAsync(TModel site);
}
