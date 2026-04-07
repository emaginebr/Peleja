namespace Peleja.Domain.Services;

using AutoMapper;
using Microsoft.Extensions.Logging;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Infra.Interfaces.Repositories;

public class PageService
{
    private readonly IPageRepository<PageModel> _pageRepository;
    private readonly ISiteRepository<SiteModel> _siteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PageService> _logger;

    public PageService(
        IPageRepository<PageModel> pageRepository,
        ISiteRepository<SiteModel> siteRepository,
        IMapper mapper,
        ILogger<PageService> logger)
    {
        _pageRepository = pageRepository;
        _siteRepository = siteRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResult<PageResult>> GetBySiteIdAsync(long siteId, long userId, long? cursor, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        // Validate site ownership
        var sites = await _siteRepository.GetByUserIdAsync(userId);
        var site = sites.FirstOrDefault(s => s.SiteId == siteId);
        if (site == null)
            throw new KeyNotFoundException("Site not found");
        if (!site.IsOwnedBy(userId))
            throw new UnauthorizedAccessException("Only the site administrator can view pages");

        var pagesWithCounts = await _pageRepository.GetBySiteIdWithCommentsAsync(siteId, cursor, pageSize);

        var hasMore = pagesWithCounts.Count > pageSize;
        if (hasMore)
            pagesWithCounts = pagesWithCounts.Take(pageSize).ToList();

        var items = pagesWithCounts.Select(p =>
        {
            var result = _mapper.Map<PageResult>(p.Page);
            result.CommentCount = p.CommentCount;
            return result;
        }).ToList();

        string? nextCursor = null;
        if (hasMore && pagesWithCounts.Count > 0)
            nextCursor = pagesWithCounts.Last().Page.PageId.ToString();

        return new PaginatedResult<PageResult>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
