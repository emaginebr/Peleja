namespace Peleja.API.Middleware;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Peleja.Infra.Interfaces.Repositories;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, ITenantRepository tenantRepository)
    {
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantSlug) || string.IsNullOrWhiteSpace(tenantSlug))
        {
            await WriteProblemResponse(context, 400, "Header X-Tenant-Id é obrigatório");
            return;
        }

        var tenant = await tenantRepository.GetBySlugAsync(tenantSlug!);

        if (tenant == null || !tenant.IsActive)
        {
            await WriteProblemResponse(context, 400, "Tenant não encontrado ou inativo");
            return;
        }

        tenantContext.TenantId = tenant.TenantId;
        tenantContext.Tenant = tenant;

        await _next(context);
    }

    private static async Task WriteProblemResponse(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Bad Request",
            Detail = detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
