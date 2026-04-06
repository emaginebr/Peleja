namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class CommentResult
{
    [JsonPropertyName("commentId")]
    public long CommentId { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("gifUrl")]
    public string? GifUrl { get; set; }

    [JsonPropertyName("isEdited")]
    public bool IsEdited { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; set; }

    [JsonPropertyName("isLikedByUser")]
    public bool IsLikedByUser { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("parentCommentId")]
    public long? ParentCommentId { get; set; }

    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("replies")]
    public List<CommentResult>? Replies { get; set; }
}
