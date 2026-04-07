namespace Peleja.API.Middleware;

using Microsoft.Extensions.Options;
using NAuth.DTO.Settings;
using Peleja.Domain.Models;
using Peleja.Infra.Interfaces.Repositories;

public class ClientIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientIdMiddleware> _logger;

    public ClientIdMiddleware(RequestDelegate next, ILogger<ClientIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISiteRepository<SiteModel> siteRepository,
        IConfiguration configuration,
        IOptionsMonitor<NAuthSetting> nauthOptions)
    {
        if (context.Request.Headers.TryGetValue("X-Client-Id", out var clientId)
            && !string.IsNullOrWhiteSpace(clientId))
        {
            var site = await siteRepository.GetByClientIdAsync(clientId.ToString());

            if (site == null)
            {
                _logger.LogWarning("Site not found for ClientId={ClientId}", clientId);
                context.Items["TenantId"] = string.Empty;
                context.Items["SiteId"] = 0L;
                await _next(context);
                return;
            }

            if (site.IsBlocked())
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Site is blocked");
                return;
            }

            context.Items["TenantId"] = site.Tenant;
            context.Items["SiteId"] = site.SiteId;
            context.Items["SiteUserId"] = site.UserId;
            context.Items["SiteAllowsWrite"] = site.AllowsWrite();

            var jwtSecret = configuration[$"Tenants:{site.Tenant}:JwtSecret"];
            var bucketName = configuration[$"Tenants:{site.Tenant}:BucketName"];

            if (!string.IsNullOrEmpty(jwtSecret))
                nauthOptions.CurrentValue.JwtSecret = jwtSecret;

            if (!string.IsNullOrEmpty(bucketName))
                nauthOptions.CurrentValue.BucketName = bucketName;
        }

        await _next(context);
    }
}
