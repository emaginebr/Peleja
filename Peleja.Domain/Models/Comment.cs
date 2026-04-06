namespace Peleja.Domain.Models;

public class Comment
{
    public long CommentId { get; set; }
    public long TenantId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? GifUrl { get; set; }
    public int LikeCount { get; set; } = 0;
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
