namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class CommentLikeResult
{
    [JsonPropertyName("commentId")]
    public long CommentId { get; set; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; set; }

    [JsonPropertyName("isLikedByUser")]
    public bool IsLikedByUser { get; set; }
}
