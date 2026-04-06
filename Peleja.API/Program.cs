using AspNetCoreRateLimit;
using NAuth.ACL;
using Peleja.API.Middleware;
using Peleja.Application;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Application layer DI (Tenant, DbContext, Repositories, Services, HttpClient)
builder.Services.AddApplication(configuration);

// NAuth
builder.Services.AddNAuth();

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
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.SetIsOriginAllowed(origin => false)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Authentication & Authorization
builder.Services.AddAuthentication();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
