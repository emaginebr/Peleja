namespace Peleja.Application.Services;

using Microsoft.Extensions.Configuration;
using NAuth.ACL.Interfaces;

public class TenantSecretProvider : ITenantSecretProvider
{
    private readonly IConfiguration _configuration;

    public TenantSecretProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetJwtSecret(string tenantId)
    {
        var secret = _configuration[$"Tenants:{tenantId}:JwtSecret"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException(
                $"JwtSecret not found for tenant '{tenantId}'. Expected key: Tenants:{tenantId}:JwtSecret");
        return secret;
    }
}
