namespace Peleja.API.Middleware;

using Peleja.Domain.Models;

public class TenantContext
{
    public long TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
