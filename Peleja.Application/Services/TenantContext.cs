namespace Peleja.Application.Services;

using Microsoft.AspNetCore.Http;
using Peleja.Application.Interfaces;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                throw new InvalidOperationException("No active HTTP context.");

            // From ClientIdMiddleware (resolved from Site.Tenant)
            if (context.Items.TryGetValue("TenantId", out var tenantId)
                && tenantId is string tenantStr
                && !string.IsNullOrEmpty(tenantStr))
                return tenantStr;

            // From JWT claim
            var claimTenant = context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(claimTenant))
                return claimTenant;

            // From X-Tenant-Id header (site admin endpoints)
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant)
                && !string.IsNullOrWhiteSpace(headerTenant))
                return headerTenant.ToString();

            throw new InvalidOperationException(
                "TenantId could not be resolved. Provide X-Client-Id or X-Tenant-Id header.");
        }
    }

    public long SiteId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                throw new InvalidOperationException("No active HTTP context.");

            if (context.Items.TryGetValue("SiteId", out var siteId) && siteId is long id)
                return id;

            return 0;
        }
    }
}
