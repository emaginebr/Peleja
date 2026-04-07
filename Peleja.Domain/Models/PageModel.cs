namespace Peleja.Domain.Models;

public class PageModel
{
    public long PageId { get; set; }
    public long SiteId { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static PageModel Create(long siteId, string pageUrl)
    {
        return new PageModel
        {
            SiteId = siteId,
            PageUrl = pageUrl,
            CreatedAt = DateTime.Now
        };
    }

    public void Update()
    {
        UpdatedAt = DateTime.Now;
    }
}
