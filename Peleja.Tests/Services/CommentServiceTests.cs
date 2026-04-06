namespace Peleja.Tests.Services;

using FluentAssertions;
using Moq;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;
using Peleja.Domain.Models.DTOs;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<ICommentLikeRepository> _commentLikeRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ICommentRepository>();
        _commentLikeRepoMock = new Mock<ICommentLikeRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new CommentService(
            _commentRepoMock.Object,
            _commentLikeRepoMock.Object,
            _userRepoMock.Object);
    }

    #region GetByPageUrlAsync

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var tenantId = 1L;
        var pageUrl = "https://example.com/page";
        var user = new User { UserId = 10, DisplayName = "TestUser", AvatarUrl = null };
        var comments = new List<Comment>
        {
            new Comment
            {
                CommentId = 1, TenantId = tenantId, UserId = 10, PageUrl = pageUrl,
                Content = "Comment 1", CreatedAt = DateTime.UtcNow, User = user,
                Replies = new List<Comment>()
            },
            new Comment
            {
                CommentId = 2, TenantId = tenantId, UserId = 10, PageUrl = pageUrl,
                Content = "Comment 2", CreatedAt = DateTime.UtcNow, User = user,
                Replies = new List<Comment>()
            }
        };

        _commentRepoMock
            .Setup(r => r.GetByPageUrlAsync(tenantId, pageUrl, "recent", null, 15))
            .ReturnsAsync(comments);

        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetByPageUrlAsync(tenantId, pageUrl, "recent", null, 15, null);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetByPageUrlAsync_WithPopularSort_ReturnsCursorWithLikeCountAndId()
    {
        // Arrange
        var tenantId = 1L;
        var pageUrl = "https://example.com/page";
        var user = new User { UserId = 10, DisplayName = "TestUser" };

        // Return pageSize + 1 items to trigger hasMore
        var comments = Enumerable.Range(1, 16).Select(i => new Comment
        {
            CommentId = i, TenantId = tenantId, UserId = 10, PageUrl = pageUrl,
            Content = $"Comment {i}", LikeCount = 100 - i, CreatedAt = DateTime.UtcNow,
            User = user, Replies = new List<Comment>()
        }).ToList();

        _commentRepoMock
            .Setup(r => r.GetByPageUrlAsync(tenantId, pageUrl, "popular", null, 15))
            .ReturnsAsync(comments);

        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetByPageUrlAsync(tenantId, pageUrl, "popular", null, 15, null);

        // Assert
        result.HasMore.Should().BeTrue();
        result.Items.Should().HaveCount(15);
        result.NextCursor.Should().Be($"{comments[14].LikeCount}_{comments[14].CommentId}");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        // Arrange
        var tenantId = 1L;
        var userId = 10L;
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = "This is a valid comment"
        };
        var user = new User { UserId = userId, DisplayName = "TestUser" };

        _commentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) =>
            {
                c.CommentId = 1;
                c.User = user;
                c.Replies = new List<Comment>();
                return c;
            });

        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateAsync(tenantId, userId, info);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("This is a valid comment");
        result.CommentId.Should().Be(1);
        _commentRepoMock.Verify(r => r.CreateAsync(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = ""
        };

        // Act
        var act = () => _service.CreateAsync(1, 10, info);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*conteúdo*obrigatório*");
    }

    [Fact]
    public async Task CreateAsync_WithContentOver2000Chars_ThrowsArgumentException()
    {
        // Arrange
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = new string('A', 2001)
        };

        // Act
        var act = () => _service.CreateAsync(1, 10, info);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*máximo 2000*");
    }

    [Fact]
    public async Task CreateAsync_WithValidParentCommentId_SucceedsAsReply()
    {
        // Arrange
        var tenantId = 1L;
        var userId = 10L;
        var parentComment = new Comment
        {
            CommentId = 100, TenantId = tenantId, UserId = 5,
            PageUrl = "https://example.com/page", Content = "Parent",
            ParentCommentId = null
        };
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = "This is a reply",
            ParentCommentId = 100
        };
        var user = new User { UserId = userId, DisplayName = "TestUser" };

        _commentRepoMock
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(parentComment);

        _commentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) =>
            {
                c.CommentId = 2;
                c.User = user;
                c.Replies = new List<Comment>();
                return c;
            });

        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<long>(), userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateAsync(tenantId, userId, info);

        // Assert
        result.Should().NotBeNull();
        result.ParentCommentId.Should().Be(100);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentParentCommentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = "This is a reply",
            ParentCommentId = 999
        };

        _commentRepoMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Comment?)null);

        // Act
        var act = () => _service.CreateAsync(1, 10, info);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*pai*não encontrado*");
    }

    [Fact]
    public async Task CreateAsync_WithParentThatIsAReply_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = 1L;
        var parentComment = new Comment
        {
            CommentId = 100, TenantId = tenantId, UserId = 5,
            PageUrl = "https://example.com/page", Content = "A reply itself",
            ParentCommentId = 50 // This is already a reply
        };
        var info = new CommentInsertInfo
        {
            PageUrl = "https://example.com/page",
            Content = "Trying to reply to a reply",
            ParentCommentId = 100
        };

        _commentRepoMock
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(parentComment);

        // Act
        var act = () => _service.CreateAsync(tenantId, 10, info);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*responder a uma resposta*");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ByAuthor_SucceedsAndSetsIsEdited()
    {
        // Arrange
        var userId = 10L;
        var comment = new Comment
        {
            CommentId = 1, UserId = userId, Content = "Original content",
            PageUrl = "https://example.com/page", IsEdited = false,
            User = new User { UserId = userId, DisplayName = "TestUser" },
            Replies = new List<Comment>()
        };
        var info = new CommentUpdateInfo { Content = "Updated content" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);
        _commentLikeRepoMock
            .Setup(r => r.ExistsAsync(1, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateAsync(1, userId, info);

        // Assert
        result.Content.Should().Be("Updated content");
        result.IsEdited.Should().BeTrue();
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c => c.IsEdited == true)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ByNonAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 10, Content = "Original",
            PageUrl = "https://example.com/page"
        };
        var info = new CommentUpdateInfo { Content = "Hacked content" };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act
        var act = () => _service.UpdateAsync(1, 99, info);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*autor*editar*");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ByAuthor_DoesSoftDelete()
    {
        // Arrange
        var userId = 10L;
        var comment = new Comment
        {
            CommentId = 1, UserId = userId, Content = "To be deleted",
            PageUrl = "https://example.com/page"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act
        await _service.DeleteAsync(1, userId, (int)UserRole.User);

        // Assert
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c =>
            c.IsDeleted == true && c.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByModerator_DoesSoftDelete()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 10, Content = "To be moderated",
            PageUrl = "https://example.com/page"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act - moderator (userId=99) deleting someone else's comment
        await _service.DeleteAsync(1, 99, (int)UserRole.Moderator);

        // Assert
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c =>
            c.IsDeleted == true && c.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByNonAuthorNonModerator_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 10, Content = "Protected",
            PageUrl = "https://example.com/page"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act - non-author, non-moderator
        var act = () => _service.DeleteAsync(1, 99, (int)UserRole.User);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*excluir*");
    }

    #endregion
}
