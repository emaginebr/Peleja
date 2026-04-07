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

    public async Task<PageModel?> GetByIdAndSiteIdAsync(long pageId, long siteId)
    {
        var entity = await _context.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PageId == pageId && p.SiteId == siteId);

        return entity != null ? _mapper.Map<PageModel>(entity) : null;
    }

    public async Task<List<(PageModel Page, int CommentCount)>> GetBySiteIdWithCommentsAsync(long siteId, long? cursor, int pageSize)
    {
        var query = _context.Pages
            .AsNoTracking()
            .Where(p => p.SiteId == siteId && p.Comments.Any(c => !c.IsDeleted));

        if (cursor.HasValue)
            query = query.Where(p => p.PageId < cursor.Value);

        var results = await query
            .OrderByDescending(p => p.PageId)
            .Take(pageSize + 1)
            .Select(p => new
            {
                Page = p,
                CommentCount = p.Comments.Count(c => !c.IsDeleted)
            })
            .ToListAsync();

        return results.Select(r => (_mapper.Map<PageModel>(r.Page), r.CommentCount)).ToList();
    }

    public async Task<PageModel> CreateAsync(PageModel page)
    {
        var entity = _mapper.Map<Page>(page);
        _context.Pages.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<PageModel>(entity);
    }
}
