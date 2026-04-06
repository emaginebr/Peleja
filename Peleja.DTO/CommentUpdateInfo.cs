namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class CommentUpdateInfo
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("gifUrl")]
    public string? GifUrl { get; set; }
}
