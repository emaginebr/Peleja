namespace Peleja.Infra.Context;

public class Comment
{
    public long CommentId { get; set; }
    public long PageId { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserImageUrl { get; set; }
    public long? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? GifUrl { get; set; }
    public int LikeCount { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Page Page { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
