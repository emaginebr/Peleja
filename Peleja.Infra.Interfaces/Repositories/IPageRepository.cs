namespace Peleja.Infra.Interfaces.Repositories;

public interface IPageRepository<TModel>
{
    Task<TModel?> GetByUrlAsync(string pageUrl);
    Task<TModel> CreateAsync(TModel page);
}
