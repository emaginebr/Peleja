namespace Peleja.Infra.Context;

using Peleja.Domain.Enums;

public class Site
{
    public long SiteId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public long UserId { get; set; }
    public SiteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
