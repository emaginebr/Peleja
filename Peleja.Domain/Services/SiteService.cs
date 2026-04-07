namespace Peleja.Domain.Services;

using AutoMapper;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Infra.Interfaces.Repositories;

public class SiteService
{
    private readonly ISiteRepository<SiteModel> _siteRepository;
    private readonly IMapper _mapper;

    public SiteService(ISiteRepository<SiteModel> siteRepository, IMapper mapper)
    {
        _siteRepository = siteRepository;
        _mapper = mapper;
    }

    public async Task<SiteResult> CreateAsync(long userId, SiteInsertInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.SiteUrl))
            throw new ArgumentException("Site URL is required");
        if (info.SiteUrl.Length > 2000)
            throw new ArgumentException("Site URL must not exceed 2000 characters");
        if (string.IsNullOrWhiteSpace(info.Tenant))
            throw new ArgumentException("Tenant is required");

        var existing = await _siteRepository.GetByUrlAsync(info.SiteUrl);
        if (existing != null)
            throw new InvalidOperationException("A site with this URL is already registered");

        var site = SiteModel.Create(info.SiteUrl, info.Tenant, userId);
        var created = await _siteRepository.CreateAsync(site);
        return _mapper.Map<SiteResult>(created);
    }

    public async Task<List<SiteResult>> ListByUserIdAsync(long userId)
    {
        var sites = await _siteRepository.GetByUserIdAsync(userId);
        return _mapper.Map<List<SiteResult>>(sites);
    }

    public async Task<SiteResult> UpdateAsync(long siteId, long userId, SiteUpdateInfo info)
    {
        var site = await _siteRepository.GetByClientIdAsync(siteId.ToString());
        if (site == null)
        {
            var sites = await _siteRepository.GetByUserIdAsync(userId);
            site = sites.FirstOrDefault(s => s.SiteId == siteId);
        }

        if (site == null)
            throw new KeyNotFoundException("Site not found");
        if (!site.IsOwnedBy(userId))
            throw new UnauthorizedAccessException("Only the site administrator can update this site");

        if (!string.IsNullOrWhiteSpace(info.SiteUrl))
        {
            if (info.SiteUrl.Length > 2000)
                throw new ArgumentException("Site URL must not exceed 2000 characters");

            var existing = await _siteRepository.GetByUrlAsync(info.SiteUrl);
            if (existing != null && existing.SiteId != site.SiteId)
                throw new InvalidOperationException("A site with this URL is already registered");

            site.Update(info.SiteUrl);
        }

        if (info.Status.HasValue)
        {
            var status = (SiteStatus)info.Status.Value;
            if (status == SiteStatus.Active) site.Activate();
            else if (status == SiteStatus.Inactive) site.Deactivate();
        }

        var updated = await _siteRepository.UpdateAsync(site);
        return _mapper.Map<SiteResult>(updated);
    }
}
