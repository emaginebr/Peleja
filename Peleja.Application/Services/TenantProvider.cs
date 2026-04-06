namespace Peleja.Application.Services;

using Microsoft.AspNetCore.Http;
using NAuth.ACL.Interfaces;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return string.Empty;

        // From JWT claim
        var claimTenant = context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(claimTenant))
            return claimTenant;

        // From middleware
        if (context.Items.TryGetValue("TenantId", out var headerTenant)
            && headerTenant is string tenantStr
            && !string.IsNullOrEmpty(tenantStr))
            return tenantStr;

        return string.Empty;
    }
}
