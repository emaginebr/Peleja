namespace Peleja.Tests.Services;

using FluentAssertions;
using Moq;
using Peleja.Domain.Models;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class CommentLikeServiceTests
{
    private readonly Mock<ICommentLikeRepository> _commentLikeRepoMock;
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly CommentLikeService _service;

    public CommentLikeServiceTests()
    {
        _commentLikeRepoMock = new Mock<ICommentLikeRepository>();
        _commentRepoMock = new Mock<ICommentRepository>();
        _service = new CommentLikeService(_commentLikeRepoMock.Object, _commentRepoMock.Object);
    }

    [Fact]
    public async Task ToggleLikeAsync_AddsLike_WhenNotLiked()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 5, LikeCount = 3,
            PageUrl = "https://example.com/page", Content = "Test"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync((CommentLike?)null);
        _commentLikeRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<CommentLike>()))
            .ReturnsAsync((CommentLike cl) => cl);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act
        var result = await _service.ToggleLikeAsync(1, 10);

        // Assert
        result.IsLikedByUser.Should().BeTrue();
        result.LikeCount.Should().Be(4);
        result.CommentId.Should().Be(1);
        _commentLikeRepoMock.Verify(r => r.CreateAsync(It.IsAny<CommentLike>()), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_RemovesLike_WhenAlreadyLiked()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 5, LikeCount = 3,
            PageUrl = "https://example.com/page", Content = "Test"
        };
        var existingLike = new CommentLike
        {
            CommentLikeId = 50, CommentId = 1, UserId = 10, CreatedAt = DateTime.UtcNow
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync(existingLike);
        _commentLikeRepoMock
            .Setup(r => r.DeleteAsync(existingLike))
            .Returns(Task.CompletedTask);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act
        var result = await _service.ToggleLikeAsync(1, 10);

        // Assert
        result.IsLikedByUser.Should().BeFalse();
        result.LikeCount.Should().Be(2);
        _commentLikeRepoMock.Verify(r => r.DeleteAsync(existingLike), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_IncrementsLikeCount_WhenAdding()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 5, LikeCount = 0,
            PageUrl = "https://example.com/page", Content = "Test"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync((CommentLike?)null);
        _commentLikeRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<CommentLike>()))
            .ReturnsAsync((CommentLike cl) => cl);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act
        var result = await _service.ToggleLikeAsync(1, 10);

        // Assert
        result.LikeCount.Should().Be(1);
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c => c.LikeCount == 1)), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_DecrementsLikeCount_WhenRemoving()
    {
        // Arrange
        var comment = new Comment
        {
            CommentId = 1, UserId = 5, LikeCount = 5,
            PageUrl = "https://example.com/page", Content = "Test"
        };
        var existingLike = new CommentLike
        {
            CommentLikeId = 50, CommentId = 1, UserId = 10
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync(existingLike);
        _commentLikeRepoMock.Setup(r => r.DeleteAsync(existingLike)).Returns(Task.CompletedTask);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        // Act
        var result = await _service.ToggleLikeAsync(1, 10);

        // Assert
        result.LikeCount.Should().Be(4);
        _commentRepoMock.Verify(r => r.UpdateAsync(It.Is<Comment>(c => c.LikeCount == 4)), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_ThrowsKeyNotFoundException_ForNonExistentComment()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Comment?)null);

        // Act
        var act = () => _service.ToggleLikeAsync(999, 10);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*não encontrado*");
    }
}
