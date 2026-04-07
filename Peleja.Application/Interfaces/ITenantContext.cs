namespace Peleja.Application.Interfaces;

public interface ITenantContext
{
    string TenantId { get; }
    long SiteId { get; }
}
