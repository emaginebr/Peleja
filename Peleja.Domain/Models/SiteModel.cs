namespace Peleja.Domain.Models;

using Peleja.Domain.Enums;

public class SiteModel
{
    public long SiteId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public long UserId { get; set; }
    public SiteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static SiteModel Create(string siteUrl, string tenant, long userId)
    {
        return new SiteModel
        {
            ClientId = Guid.NewGuid().ToString("N"),
            SiteUrl = siteUrl,
            Tenant = tenant,
            UserId = userId,
            Status = SiteStatus.Active,
            CreatedAt = DateTime.Now
        };
    }

    public void Update(string siteUrl)
    {
        SiteUrl = siteUrl;
        UpdatedAt = DateTime.Now;
    }

    public void Activate()
    {
        Status = SiteStatus.Active;
        UpdatedAt = DateTime.Now;
    }

    public void Deactivate()
    {
        Status = SiteStatus.Inactive;
        UpdatedAt = DateTime.Now;
    }

    public void Block()
    {
        Status = SiteStatus.Blocked;
        UpdatedAt = DateTime.Now;
    }

    public bool IsActive() => Status == SiteStatus.Active;

    public bool IsBlocked() => Status == SiteStatus.Blocked;

    public bool IsOwnedBy(long userId) => UserId == userId;

    public bool AllowsRead() => Status != SiteStatus.Blocked;

    public bool AllowsWrite() => Status == SiteStatus.Active;
}
