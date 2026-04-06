namespace Peleja.Tests.Services;

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
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<ICommentLikeRepository> _commentLikeRepoMock;
    private readonly Mock<IPageRepository> _pageRepoMock;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ICommentRepository>();
        _commentLikeRepoMock = new Mock<ICommentLikeRepository>();
        _pageRepoMock = new Mock<IPageRepository>();
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

    #region GetByPageUrlAsync

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsPaginatedResults()
    {
        var pageUrl = "https://example.com/page";
        var page = new PageModel { PageId = 1, UserId = 1, PageUrl = pageUrl };
        var comments = new List<CommentModel>
        {
            new CommentModel
            {
                CommentId = 1, PageId = 1, UserId = 10,
                Content = "Comment 1", CreatedAt = DateTime.UtcNow,
                Replies = new List<CommentModel>()
            },
            new CommentModel
            {
                CommentId = 2, PageId = 1, UserId = 10,
                Content = "Comment 2", CreatedAt = DateTime.UtcNow,
                Replies = new List<CommentModel>()
            }
        };

        _pageRepoMock.Setup(r => r.GetByUrlAsync(pageUrl)).ReturnsAsync(page);
        _commentRepoMock
            .Setup(r => r.GetByPageIdAsync(1, "recent", null, 15))
            .ReturnsAsync(comments);
        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(false);

        var result = await _service.GetByPageUrlAsync(pageUrl, "recent", null, 15, null);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsEmpty_WhenPageNotFound()
    {
        _pageRepoMock.Setup(r => r.GetByUrlAsync("https://unknown.com")).ReturnsAsync((PageModel?)null);

        var result = await _service.GetByPageUrlAsync("https://unknown.com", "recent", null, 15, null);

        result.Items.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesPageAndComment()
    {
        var userId = 10L;
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/new-page",
            Content = "This is a valid comment"
        };

        _pageRepoMock.Setup(r => r.GetByUrlAsync(info.PageUrl)).ReturnsAsync((PageModel?)null);
        _pageRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<PageModel>()))
            .ReturnsAsync((PageModel p) => { p.PageId = 1; return p; });

        _commentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) =>
            {
                c.CommentId = 1;
                c.Replies = new List<CommentModel>();
                return c;
            });

        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), userId))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(userId, info);

        result.Should().NotBeNull();
        result.Content.Should().Be("This is a valid comment");
        _pageRepoMock.Verify(r => r.CreateAsync(It.IsAny<PageModel>()), Times.Once);
        _commentRepoMock.Verify(r => r.CreateAsync(It.IsAny<CommentModel>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyContent_ThrowsArgumentException()
    {
        var info = new CommentInsertInfo { PageUrl = "https://example.com/page", Content = "" };

        var act = () => _service.CreateAsync(10, info);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*conteúdo*obrigatório*");
    }

    [Fact]
    public async Task CreateAsync_WithContentOver2000Chars_ThrowsArgumentException()
    {
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = new string('A', 2001)
        };

        var act = () => _service.CreateAsync(10, info);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*máximo 2000*");
    }

    [Fact]
    public async Task CreateAsync_WithParentThatIsAReply_ThrowsArgumentException()
    {
        var page = new PageModel { PageId = 1, UserId = 1, PageUrl = "https://example.com/page" };
        var parentComment = new CommentModel
        {
            CommentId = 100, PageId = 1, UserId = 5,
            Content = "A reply itself",
            ParentCommentId = 50
        };
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = "Trying to reply to a reply",
            ParentCommentId = 100
        };

        _pageRepoMock.Setup(r => r.GetByUrlAsync(info.PageUrl)).ReturnsAsync(page);
        _commentRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(parentComment);

        var act = () => _service.CreateAsync(10, info);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*responder a uma resposta*");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ByAuthor_SucceedsAndSetsIsEdited()
    {
        var userId = 10L;
        var comment = new CommentModel
        {
            CommentId = 1, PageId = 1, UserId = userId, Content = "Original content",
            IsEdited = false, Replies = new List<CommentModel>()
        };
        var info = new CommentUpdateInfo { Content = "Updated content" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) => c);
        _commentLikeRepoMock.Setup(r => r.ExistsAsync(1, userId)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, userId, info);

        result.Content.Should().Be("Updated content");
        result.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ByNonAuthor_ThrowsUnauthorizedAccessException()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Original" };
        var info = new CommentUpdateInfo { Content = "Hacked content" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        var act = () => _service.UpdateAsync(1, 99, info);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*autor*editar*");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ByAuthor_DoesSoftDelete()
    {
        var userId = 10L;
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = userId, Content = "To be deleted" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) => c);

        await _service.DeleteAsync(1, userId, false);

        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<CommentModel>(c =>
            c.IsDeleted == true && c.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByAdmin_DoesSoftDelete()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "To be moderated" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) => c);

        await _service.DeleteAsync(1, 99, true);

        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<CommentModel>(c =>
            c.IsDeleted == true && c.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByNonAuthorNonAdmin_ThrowsUnauthorizedAccessException()
    {
        var comment = new CommentModel { CommentId = 1, PageId = 1, UserId = 10, Content = "Protected" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        var act = () => _service.DeleteAsync(1, 99, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*excluir*");
    }

    #endregion
}
