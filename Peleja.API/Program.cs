using AspNetCoreRateLimit;
using NAuth.ACL;
using NAuth.ACL.Interfaces;
using Peleja.API.Middleware;
using Peleja.Application;
using Peleja.Application.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Application layer DI (Tenant, DbContext, Repositories, Services, HttpClient)
builder.Services.AddApplication(configuration);

// NAuth multi-tenant providers
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ITenantSecretProvider, TenantSecretProvider>();

// NAuth
builder.Services.AddNAuth();
builder.Services.AddNAuthAuthentication("BasicAuthentication");

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsProduction())
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseIpRateLimiting();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<ClientIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
