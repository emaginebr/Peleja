namespace Peleja.Domain.Models;

public class CommentLike
{
    public long CommentLikeId { get; set; }
    public long CommentId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}
