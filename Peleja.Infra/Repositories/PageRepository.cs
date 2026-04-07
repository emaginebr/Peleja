namespace Peleja.Infra.Repositories;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class PageRepository : IPageRepository<PageModel>
{
    private readonly PelejaContext _context;
    private readonly IMapper _mapper;

    public PageRepository(PelejaContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PageModel?> GetByUrlAndSiteIdAsync(long siteId, string pageUrl)
    {
        var entity = await _context.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SiteId == siteId && p.PageUrl == pageUrl);

        return entity != null ? _mapper.Map<PageModel>(entity) : null;
    }

    public async Task<PageModel> CreateAsync(PageModel page)
    {
        var entity = _mapper.Map<Page>(page);
        _context.Pages.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<PageModel>(entity);
    }
}
