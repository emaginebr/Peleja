namespace Peleja.Domain.Services;

using Peleja.DTO;
using Peleja.Domain.Interfaces.AppServices;

public class GiphyService
{
    private readonly IGiphyAppService _giphyAppService;

    public GiphyService(IGiphyAppService giphyAppService)
    {
        _giphyAppService = giphyAppService;
    }

    public async Task<GiphySearchResult> SearchAsync(string query, int limit = 20, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("O termo de busca é obrigatório");

        limit = Math.Clamp(limit, 1, 50);
        offset = Math.Max(0, offset);

        return await _giphyAppService.SearchAsync(query, limit, offset);
    }
}
