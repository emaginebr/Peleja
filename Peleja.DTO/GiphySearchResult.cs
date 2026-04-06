namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class GiphySearchResult
{
    [JsonPropertyName("items")]
    public List<GiphyItemInfo> Items { get; set; } = new List<GiphyItemInfo>();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}
