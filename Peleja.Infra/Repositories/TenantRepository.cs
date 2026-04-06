namespace Peleja.Infra.Repositories;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly PelejaContext _context;

    public TenantRepository(PelejaContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<Tenant?> GetByIdAsync(long tenantId)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
    }
}
