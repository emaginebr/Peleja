namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class PageResult
{
    [JsonPropertyName("pageId")]
    public long PageId { get; set; }

    [JsonPropertyName("pageUrl")]
    public string PageUrl { get; set; } = string.Empty;

    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
