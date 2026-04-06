namespace Peleja.Domain.Models.DTOs;

using System.Text.Json.Serialization;

public class CommentInsertInfo
{
    [JsonPropertyName("pageUrl")]
    public string PageUrl { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("gifUrl")]
    public string? GifUrl { get; set; }

    [JsonPropertyName("parentCommentId")]
    public long? ParentCommentId { get; set; }
}
