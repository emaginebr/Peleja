namespace Peleja.Infra.Repositories;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class SiteRepository : ISiteRepository<SiteModel>
{
    private readonly PelejaContext _context;
    private readonly IMapper _mapper;

    public SiteRepository(PelejaContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SiteModel?> GetByClientIdAsync(string clientId)
    {
        var entity = await _context.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ClientId == clientId);

        return entity != null ? _mapper.Map<SiteModel>(entity) : null;
    }

    public async Task<SiteModel?> GetByUrlAsync(string siteUrl)
    {
        var entity = await _context.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SiteUrl == siteUrl);

        return entity != null ? _mapper.Map<SiteModel>(entity) : null;
    }

    public async Task<List<SiteModel>> GetByUserIdAsync(long userId)
    {
        var entities = await _context.Sites
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<SiteModel>>(entities);
    }

    public async Task<List<SiteModel>> GetByUserIdPaginatedAsync(long userId, long? cursor, int pageSize)
    {
        var query = _context.Sites
            .AsNoTracking()
            .Where(s => s.UserId == userId);

        if (cursor.HasValue)
            query = query.Where(s => s.SiteId < cursor.Value);

        var entities = await query
            .OrderByDescending(s => s.SiteId)
            .Take(pageSize + 1)
            .ToListAsync();

        return _mapper.Map<List<SiteModel>>(entities);
    }

    public async Task<SiteModel> CreateAsync(SiteModel site)
    {
        var entity = _mapper.Map<Site>(site);
        _context.Sites.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<SiteModel>(entity);
    }

    public async Task<SiteModel> UpdateAsync(SiteModel site)
    {
        var entity = await _context.Sites
            .FirstOrDefaultAsync(s => s.SiteId == site.SiteId);

        if (entity == null)
            throw new KeyNotFoundException("Site not found");

        _mapper.Map(site, entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<SiteModel>(entity);
    }
}
