namespace Peleja.Tests.Domain.Services;

using AutoMapper;
using FluentAssertions;
using Moq;
using Peleja.Domain.Enums;
using Peleja.Domain.Mappings;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class SiteServiceTests
{
    private readonly Mock<ISiteRepository<SiteModel>> _siteRepoMock;
    private readonly SiteService _service;

    public SiteServiceTests()
    {
        _siteRepoMock = new Mock<ISiteRepository<SiteModel>>();
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SiteResultProfile>();
        }).CreateMapper();
        _service = new SiteService(_siteRepoMock.Object, mapper);
    }

    [Fact]
    public async Task ListByUserIdAsync_ReturnsPaginatedResults()
    {
        var sites = Enumerable.Range(1, 16).Select(i => new SiteModel
        {
            SiteId = i, ClientId = $"client{i}", SiteUrl = $"https://site{i}.com",
            Tenant = "emagine", UserId = 1, Status = SiteStatus.Active, CreatedAt = DateTime.Now
        }).ToList();

        _siteRepoMock.Setup(r => r.GetByUserIdPaginatedAsync(1, null, 15)).ReturnsAsync(sites);

        var result = await _service.ListByUserIdAsync(1, null, 15);

        result.Items.Should().HaveCount(15);
        result.HasMore.Should().BeTrue();
        result.NextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task ListByUserIdAsync_ReturnsAllWhenLessThanPageSize()
    {
        var sites = Enumerable.Range(1, 3).Select(i => new SiteModel
        {
            SiteId = i, ClientId = $"client{i}", SiteUrl = $"https://site{i}.com",
            Tenant = "emagine", UserId = 1, Status = SiteStatus.Active, CreatedAt = DateTime.Now
        }).ToList();

        _siteRepoMock.Setup(r => r.GetByUserIdPaginatedAsync(1, null, 15)).ReturnsAsync(sites);

        var result = await _service.ListByUserIdAsync(1, null, 15);

        result.Items.Should().HaveCount(3);
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task ListByUserIdAsync_ReturnsEmptyWhenNoSites()
    {
        _siteRepoMock.Setup(r => r.GetByUserIdPaginatedAsync(1, null, 15)).ReturnsAsync(new List<SiteModel>());

        var result = await _service.ListByUserIdAsync(1, null, 15);

        result.Items.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSiteResult()
    {
        var info = new SiteInsertInfo { SiteUrl = "https://newsite.com", Tenant = "emagine" };
        _siteRepoMock.Setup(r => r.GetByUrlAsync(info.SiteUrl)).ReturnsAsync((SiteModel?)null);
        _siteRepoMock.Setup(r => r.CreateAsync(It.IsAny<SiteModel>())).ReturnsAsync((SiteModel s) => { s.SiteId = 1; return s; });

        var result = await _service.CreateAsync(1, info);

        result.SiteUrl.Should().Be("https://newsite.com");
        result.ClientId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_DuplicateUrl_ThrowsInvalidOperation()
    {
        var info = new SiteInsertInfo { SiteUrl = "https://existing.com", Tenant = "emagine" };
        _siteRepoMock.Setup(r => r.GetByUrlAsync(info.SiteUrl)).ReturnsAsync(new SiteModel { SiteId = 1, SiteUrl = info.SiteUrl });

        var act = () => _service.CreateAsync(1, info);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already registered*");
    }
}
