namespace Peleja.Domain.Models;

public class PageModel
{
    public long PageId { get; set; }
    public long UserId { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static PageModel Create(long userId, string pageUrl)
    {
        return new PageModel
        {
            UserId = userId,
            PageUrl = pageUrl,
            CreatedAt = DateTime.Now
        };
    }

    public void Update()
    {
        UpdatedAt = DateTime.Now;
    }
}
