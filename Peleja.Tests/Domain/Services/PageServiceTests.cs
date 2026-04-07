namespace Peleja.Tests.Domain.Services;

using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Peleja.Domain.Enums;
using Peleja.Domain.Mappings;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class PageServiceTests
{
    private readonly Mock<IPageRepository<PageModel>> _pageRepoMock;
    private readonly Mock<ISiteRepository<SiteModel>> _siteRepoMock;
    private readonly PageService _service;

    public PageServiceTests()
    {
        _pageRepoMock = new Mock<IPageRepository<PageModel>>();
        _siteRepoMock = new Mock<ISiteRepository<SiteModel>>();
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PageResultProfile>();
        }).CreateMapper();
        var logger = new Mock<ILogger<PageService>>();
        _service = new PageService(_pageRepoMock.Object, _siteRepoMock.Object, mapper, logger.Object);
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsPagesWithCommentCount()
    {
        var site = new SiteModel { SiteId = 1, UserId = 10, Status = SiteStatus.Active };
        _siteRepoMock.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync(new List<SiteModel> { site });

        var pages = new List<(PageModel Page, int CommentCount)>
        {
            (new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://site.com/post-1", CreatedAt = DateTime.Now }, 5),
            (new PageModel { PageId = 2, SiteId = 1, PageUrl = "https://site.com/post-2", CreatedAt = DateTime.Now }, 12)
        };
        _pageRepoMock.Setup(r => r.GetBySiteIdWithCommentsAsync(1, null, 15)).ReturnsAsync(pages);

        var result = await _service.GetBySiteIdAsync(1, 10, null, 15);

        result.Items.Should().HaveCount(2);
        result.Items[0].CommentCount.Should().Be(5);
        result.Items[1].CommentCount.Should().Be(12);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySiteIdAsync_ThrowsKeyNotFound_WhenSiteNotFound()
    {
        _siteRepoMock.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync(new List<SiteModel>());

        var act = () => _service.GetBySiteIdAsync(999, 10, null, 15);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Site not found*");
    }

    [Fact]
    public async Task GetBySiteIdAsync_ThrowsUnauthorized_WhenNotOwner()
    {
        var site = new SiteModel { SiteId = 1, UserId = 99, Status = SiteStatus.Active };
        _siteRepoMock.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync(new List<SiteModel> { site });

        // User 10 tries to access site owned by user 99
        var act = () => _service.GetBySiteIdAsync(1, 10, null, 15);

        // Site is in the list because GetByUserIdAsync returns sites for userId=10,
        // but site.UserId=99 so IsOwnedBy fails
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsPaginatedResults()
    {
        var site = new SiteModel { SiteId = 1, UserId = 10, Status = SiteStatus.Active };
        _siteRepoMock.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync(new List<SiteModel> { site });

        var pages = Enumerable.Range(1, 16).Select(i =>
            ((PageModel Page, int CommentCount))(new PageModel { PageId = i, SiteId = 1, PageUrl = $"https://site.com/post-{i}", CreatedAt = DateTime.Now }, i)
        ).ToList();
        _pageRepoMock.Setup(r => r.GetBySiteIdWithCommentsAsync(1, null, 15)).ReturnsAsync(pages);

        var result = await _service.GetBySiteIdAsync(1, 10, null, 15);

        result.Items.Should().HaveCount(15);
        result.HasMore.Should().BeTrue();
        result.NextCursor.Should().NotBeNull();
    }
}
