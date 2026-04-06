namespace Peleja.Domain.Models;

public class CommentModel
{
    public long CommentId { get; set; }
    public long PageId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? GifUrl { get; set; }
    public int LikeCount { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<CommentModel> Replies { get; set; } = new();

    public static CommentModel Create(long pageId, long userId, string content, string? gifUrl, long? parentCommentId)
    {
        return new CommentModel
        {
            PageId = pageId,
            UserId = userId,
            Content = content,
            GifUrl = gifUrl,
            ParentCommentId = parentCommentId,
            LikeCount = 0,
            IsEdited = false,
            IsDeleted = false,
            CreatedAt = DateTime.Now
        };
    }

    public void Update(string content, string? gifUrl)
    {
        Content = content;
        GifUrl = gifUrl;
        IsEdited = true;
        UpdatedAt = DateTime.Now;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
    }

    public void DecrementLikeCount()
    {
        LikeCount = Math.Max(0, LikeCount - 1);
    }

    public bool IsReply() => ParentCommentId != null;

    public bool IsOwnedBy(long userId) => UserId == userId;
}
