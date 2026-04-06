namespace Peleja.Infra.Context;

public class Page
{
    public long PageId { get; set; }
    public long UserId { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
