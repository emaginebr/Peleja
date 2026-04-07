namespace Peleja.Tests.Domain.Services;

using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        var logger = new Mock<ILogger<CommentService>>();
        _service = new CommentService(
            _commentRepoMock.Object,
            _commentLikeRepoMock.Object,
            _pageRepoMock.Object,
            mapper,
            logger.Object);
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsPaginatedResults()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var comments = new List<CommentModel>
        {
            new CommentModel { CommentId = 1, PageId = 1, UserId = 10, UserName = "John", Content = "Comment 1", CreatedAt = DateTime.Now, Replies = new() },
            new CommentModel { CommentId = 2, PageId = 1, UserId = 10, UserName = "John", Content = "Comment 2", CreatedAt = DateTime.Now, Replies = new() }
        };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://example.com/page")).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByPageIdAsync(1, "recent", null, 15)).ReturnsAsync(comments);

        var result = await _service.GetByPageUrlAsync(1, "https://example.com/page", "recent", null, 15, null);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsUserNameFromDatabase()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var comments = new List<CommentModel>
        {
            new CommentModel
            {
                CommentId = 1, PageId = 1, UserId = 10,
                UserName = "John Doe", UserImageUrl = "https://img.example.com/john.jpg",
                Content = "Hello", CreatedAt = DateTime.Now, Replies = new()
            }
        };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://example.com/page")).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByPageIdAsync(1, "recent", null, 15)).ReturnsAsync(comments);

        var result = await _service.GetByPageUrlAsync(1, "https://example.com/page", "recent", null, 15, null);

        result.Items.Should().HaveCount(1);
        result.Items[0].UserName.Should().Be("John Doe");
        result.Items[0].UserImageUrl.Should().Be("https://img.example.com/john.jpg");
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsNullUserName_ForDeletedComments()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var deletedComment = new CommentModel
        {
            CommentId = 1, PageId = 1, UserId = 10, UserName = "John Doe",
            Content = "Deleted", IsDeleted = true, CreatedAt = DateTime.Now, Replies = new()
        };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://example.com/page")).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByPageIdAsync(1, "recent", null, 15)).ReturnsAsync(new List<CommentModel> { deletedComment });

        var result = await _service.GetByPageUrlAsync(1, "https://example.com/page", "recent", null, 15, null);

        result.Items.Should().HaveCount(1);
        result.Items[0].UserName.Should().BeNull();
        result.Items[0].UserImageUrl.Should().BeNull();
        result.Items[0].Content.Should().Be("[Comment removed]");
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsEmpty_WhenPageNotFound()
    {
        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, "https://unknown.com")).ReturnsAsync((PageModel?)null);

        var result = await _service.GetByPageUrlAsync(1, "https://unknown.com", "recent", null, 15, null);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_StoresUserNameInComment()
    {
        var info = new CommentInsertInfo { PageUrl = "https://example.com/new-page", Content = "Valid comment" };

        _pageRepoMock.Setup(r => r.GetByUrlAndSiteIdAsync(1, info.PageUrl)).ReturnsAsync((PageModel?)null);
        _pageRepoMock.Setup(r => r.CreateAsync(It.IsAny<PageModel>())).ReturnsAsync((PageModel p) => { p.PageId = 1; return p; });
        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<CommentModel>())).ReturnsAsync((CommentModel c) => { c.CommentId = 1; c.Replies = new(); return c; });

        var result = await _service.CreateAsync(1, 10, "Jane Doe", "https://img.example.com/jane.jpg", info);

        result.Should().NotBeNull();
        result.Content.Should().Be("Valid comment");
        result.UserName.Should().Be("Jane Doe");
        result.UserImageUrl.Should().Be("https://img.example.com/jane.jpg");
        _commentRepoMock.Verify(r => r.CreateAsync(It.Is<CommentModel>(c =>
            c.UserName == "Jane Doe" && c.UserImageUrl == "https://img.example.com/jane.jpg")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyContent_ThrowsArgumentException()
    {
        var info = new CommentInsertInfo { PageUrl = "https://example.com/page", Content = "" };
        var act = () => _service.CreateAsync(1, 10, "User", null, info);
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

        var act = () => _service.CreateAsync(1, 10, "User", null, info);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*reply to a reply*");
    }

    [Fact]
    public async Task GetByPageIdAuthenticatedAsync_ReturnsPaginatedComments()
    {
        var page = new PageModel { PageId = 1, SiteId = 1, PageUrl = "https://example.com/page" };
        var comments = new List<CommentModel>
        {
            new CommentModel { CommentId = 1, PageId = 1, UserId = 10, UserName = "John", Content = "Comment 1", CreatedAt = DateTime.Now, Replies = new() },
            new CommentModel { CommentId = 2, PageId = 1, UserId = 10, UserName = "John", Content = "Comment 2", CreatedAt = DateTime.Now, Replies = new() }
        };

        _pageRepoMock.Setup(r => r.GetByIdAndSiteIdAsync(1, 1)).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByPageIdAsync(1, "recent", null, 15)).ReturnsAsync(comments);

        var result = await _service.GetByPageIdAuthenticatedAsync(1, 1, 10, "recent", null, 15);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPageIdAuthenticatedAsync_ThrowsKeyNotFound_WhenPageNotInSite()
    {
        _pageRepoMock.Setup(r => r.GetByIdAndSiteIdAsync(999, 1)).ReturnsAsync((PageModel?)null);

        var act = () => _service.GetByPageIdAuthenticatedAsync(1, 999, 10, "recent", null, 15);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Page not found*");
    }

    [Fact]
    public async Task UpdateAsync_ByAuthor_SucceedsAndSetsIsEdited()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, UserName = "John", Content = "Original", Replies = new() };
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
