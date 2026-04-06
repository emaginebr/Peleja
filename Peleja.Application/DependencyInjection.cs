namespace Peleja.Application;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        // DbContext
        services.AddDbContext<PelejaContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PelejaContext")));

        // Repositories - register for both Infra.Interfaces and Domain.Interfaces
        services.AddScoped<InfraInterfaces.Repositories.ITenantRepository, TenantRepository>();
        services.AddScoped<InfraInterfaces.Repositories.IUserRepository, UserRepository>();
        services.AddScoped<InfraInterfaces.Repositories.ICommentRepository, CommentRepository>();
        services.AddScoped<InfraInterfaces.Repositories.ICommentLikeRepository, CommentLikeRepository>();

        services.AddScoped<DomainInterfaces.Repositories.ITenantRepository, TenantRepository>();
        services.AddScoped<DomainInterfaces.Repositories.IUserRepository, UserRepository>();
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
