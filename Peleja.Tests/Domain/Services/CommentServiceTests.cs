namespace Peleja.Tests.Domain.Services;

using AutoMapper;
using FluentAssertions;
using Moq;
using Peleja.Domain.Mappings;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository<CommentModel>> _commentRepoMock;
    private readonly Mock<ICommentLikeRepository<CommentLikeModel>> _commentLikeRepoMock;
    private readonly Mock<IPageRepository<PageModel>> _pageRepoMock;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ICommentRepository<CommentModel>>();
        _commentLikeRepoMock = new Mock<ICommentLikeRepository<CommentLikeModel>>();
        _pageRepoMock = new Mock<IPageRepository<PageModel>>();
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CommentResultProfile>();
            cfg.AddProfile<CommentLikeResultProfile>();
        }).CreateMapper();
        _service = new CommentService(
            _commentRepoMock.Object,
            _commentLikeRepoMock.Object,
            _pageRepoMock.Object,
            mapper);
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsPaginatedResults()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var comments = new List<CommentModel>
        {
            new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Comment 1", CreatedAt = DateTime.Now, Replies = new() },
            new CommentModel { CommentId = 2, PageId = 1, UserId = 10, Content = "Comment 2", CreatedAt = DateTime.Now, Replies = new() }
        };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://example.com/page")).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByPageIdAsync(1, "recent", null, 15)).ReturnsAsync(comments);
        _commentLikeRepoMock.Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(false);

        var result = await _service.GetByPageUrlAsync(1, "https://example.com/page", "recent", null, 15, null);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsEmpty_WhenPageNotFound()
    {
        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://unknown.com")).ReturnsAsync((PageModel?)null);

        var result = await _service.GetByPageUrlAsync(1, "https://unknown.com", "recent", null, 15, null);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesPageAndComment()
    {
        var info = new CommentInsertInfo { PageUrl = "https://example.com/new-page", Content = "Valid comment" };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, info.PageUrl)).ReturnsAsync((PageModel?)null);
        _pageRepoMock.Setup(r => r.CreateAsync(It.IsAny<PageModel>())).ReturnsAsync((PageModel p) => { p.PageId = 1; return p; });
        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<CommentModel>())).ReturnsAsync((CommentModel c) => { c.CommentId = 1; c.Replies = new(); return c; });
        _commentLikeRepoMock.Setup(r => r.ExistsAsync(It.IsAny<long>(), 10)).ReturnsAsync(false);

        var result = await _service.CreateAsync(1, 10, info);

        result.Should().NotBeNull();
        result.Content.Should().Be("Valid comment");
        _pageRepoMock.Verify(r => r.CreateAsync(It.IsAny<PageModel>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyContent_ThrowsArgumentException()
    {
        var info = new CommentInsertInfo { PageUrl = "https://example.com/page", Content = "" };
        var act = () => _service.CreateAsync(1, 10, info);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Content*required*");
    }

    [Fact]
    public async Task CreateAsync_WithParentThatIsAReply_ThrowsArgumentException()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var parent = new CommentModel { CommentId = 100, PageId = 1, UserId = 5, Content = "Reply", ParentCommentId = 50 };
        var info = new CommentInsertInfo { PageUrl = "https://example.com/page", Content = "Trying to reply to a reply", ParentCommentId = 100 };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, info.PageUrl)).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(parent);

        var act = () => _service.CreateAsync(1, 10, info);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*reply to a reply*");
    }

    [Fact]
    public async Task UpdateAsync_ByAuthor_SucceedsAndSetsIsEdited()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Original", Replies = new() };
        var info = new CommentUpdateInfo { Content = "Updated content" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CommentModel>())).ReturnsAsync((CommentModel c) => c);
        _commentLikeRepoMock.Setup(r => r.ExistsAsync(1, 10)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, 10, info);
        result.Content.Should().Be("Updated content");
        result.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ByNonAuthor_ThrowsUnauthorizedAccessException()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Original" };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        var act = () => _service.UpdateAsync(1, 99, new CommentUpdateInfo { Content = "Hacked" });
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*author*edit*");
    }

    [Fact]
    public async Task DeleteAsync_ByAuthor_DoesSoftDelete()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Delete me" };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CommentModel>())).ReturnsAsync((CommentModel c) => c);

        await _service.DeleteAsync(1, 10, false);
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<CommentModel>(c => c.IsDeleted && c.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BySiteAdmin_DoesSoftDelete()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Moderated" };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CommentModel>())).ReturnsAsync((CommentModel c) => c);

        await _service.DeleteAsync(1, 99, false, siteAdminUserId: 99);
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<CommentModel>(c => c.IsDeleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByNonAuthorNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Protected" };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        var act = () => _service.DeleteAsync(1, 99, false);
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*permission*delete*");
    }
}
