namespace Peleja.Infra.AppServices;

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Peleja.Domain.Models.DTOs;
using Peleja.Infra.Interfaces.AppServices;

public class GiphyAppService : IGiphyAppService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public GiphyAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["GIPHY_API_KEY"] ?? string.Empty;
    }

    public async Task<GiphySearchResult> SearchAsync(string query, int limit = 20, int offset = 0)
    {
        var client = _httpClientFactory.CreateClient("Giphy");

        var url = $"https://api.giphy.com/v1/gifs/search?api_key={_apiKey}&q={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var result = new GiphySearchResult
            {
                TotalCount = root.GetProperty("pagination").GetProperty("total_count").GetInt32(),
                Offset = root.GetProperty("pagination").GetProperty("offset").GetInt32(),
                Limit = root.GetProperty("pagination").GetProperty("count").GetInt32()
            };

            var dataArray = root.GetProperty("data");

            foreach (var item in dataArray.EnumerateArray())
            {
                var images = item.GetProperty("images");
                var original = images.GetProperty("original");
                var fixedWidth = images.GetProperty("fixed_width");

                result.Items.Add(new GiphyItemInfo
                {
                    Id = item.GetProperty("id").GetString() ?? string.Empty,
                    Title = item.GetProperty("title").GetString() ?? string.Empty,
                    Url = original.GetProperty("url").GetString() ?? string.Empty,
                    PreviewUrl = fixedWidth.GetProperty("url").GetString() ?? string.Empty,
                    Width = int.TryParse(original.GetProperty("width").GetString(), out var w) ? w : 0,
                    Height = int.TryParse(original.GetProperty("height").GetString(), out var h) ? h : 0
                });
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("Giphy API is unavailable. Please try again later.", ex);
        }
    }
}
