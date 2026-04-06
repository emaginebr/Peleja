namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<Tenant?> GetByIdAsync(long tenantId);
}
