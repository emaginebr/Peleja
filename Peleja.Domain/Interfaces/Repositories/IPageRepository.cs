namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface IPageRepository
{
    Task<PageModel?> GetByUrlAsync(string pageUrl);
    Task<PageModel> CreateAsync(PageModel page);
}
