namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class PaginatedResult<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new List<T>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }
}
