namespace Peleja.Domain.Interfaces.AppServices;

using Peleja.Domain.Models.DTOs;

public interface IGiphyAppService
{
    Task<GiphySearchResult> SearchAsync(string query, int limit = 20, int offset = 0);
}
