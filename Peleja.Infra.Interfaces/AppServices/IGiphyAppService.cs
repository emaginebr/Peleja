namespace Peleja.Infra.Interfaces.AppServices;

using Peleja.DTO;

public interface IGiphyAppService
{
    Task<GiphySearchResult> SearchAsync(string query, int limit = 20, int offset = 0);
}
