namespace Peleja.Application;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peleja.Application.Interfaces;
using Peleja.Application.Services;
using Peleja.Domain.Services;
using Peleja.Infra.Context;
using Peleja.Infra.Repositories;
using Peleja.Infra.AppServices;
using InfraInterfaces = Peleja.Infra.Interfaces;
using DomainInterfaces = Peleja.Domain.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant Context
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // DbContext via factory (per-tenant connection string)
        services.AddScoped(sp =>
        {
            var tenantContext = sp.GetRequiredService<ITenantContext>();
            var connectionString = configuration[$"Tenants:{tenantContext.TenantId}:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"ConnectionString not found for tenant '{tenantContext.TenantId}'.");

            var optionsBuilder = new DbContextOptionsBuilder<PelejaContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new PelejaContext(optionsBuilder.Options);
        });

        // AutoMapper (scan profiles from Infra + Domain)
        services.AddAutoMapper(
            typeof(Peleja.Infra.Mappings.PageMapperProfile),
            typeof(Peleja.Domain.Mappings.CommentResultProfile));

        // Repositories
        services.AddScoped<InfraInterfaces.Repositories.IPageRepository, PageRepository>();
        services.AddScoped<InfraInterfaces.Repositories.ICommentRepository, CommentRepository>();
        services.AddScoped<InfraInterfaces.Repositories.ICommentLikeRepository, CommentLikeRepository>();

        services.AddScoped<DomainInterfaces.Repositories.IPageRepository, PageRepository>();
        services.AddScoped<DomainInterfaces.Repositories.ICommentRepository, CommentRepository>();
        services.AddScoped<DomainInterfaces.Repositories.ICommentLikeRepository, CommentLikeRepository>();

        // AppServices
        services.AddScoped<InfraInterfaces.AppServices.IGiphyAppService, GiphyAppService>();
        services.AddScoped<DomainInterfaces.AppServices.IGiphyAppService, GiphyAppService>();

        // Domain Services
        services.AddScoped<CommentService>();
        services.AddScoped<CommentLikeService>();
        services.AddScoped<GiphyService>();

        // HttpClient for Giphy
        services.AddHttpClient("Giphy");

        return services;
    }
}
