namespace Peleja.Domain.Models;

public class CommentLikeModel
{
    public long CommentLikeId { get; set; }
    public long CommentId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CommentLikeModel Create(long commentId, long userId)
    {
        return new CommentLikeModel
        {
            CommentId = commentId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
