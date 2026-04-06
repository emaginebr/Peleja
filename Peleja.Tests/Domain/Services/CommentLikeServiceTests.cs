namespace Peleja.Tests.Domain.Services;

using AutoMapper;
using FluentAssertions;
using Moq;
using Peleja.Domain.Mappings;
using Peleja.Domain.Models;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.Repositories;

public class CommentLikeServiceTests
{
    private readonly Mock<ICommentLikeRepository<CommentLikeModel>> _commentLikeRepoMock;
    private readonly Mock<ICommentRepository<CommentModel>> _commentRepoMock;
    private readonly CommentLikeService _service;

    public CommentLikeServiceTests()
    {
        _commentLikeRepoMock = new Mock<ICommentLikeRepository<CommentLikeModel>>();
        _commentRepoMock = new Mock<ICommentRepository<CommentModel>>();
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CommentResultProfile>();
            cfg.AddProfile<CommentLikeResultProfile>();
        }).CreateMapper();
        _service = new CommentLikeService(_commentLikeRepoMock.Object, _commentRepoMock.Object, mapper);
    }

    [Fact]
    public async Task ToggleLikeAsync_AddsLike_WhenNotLiked()
    {
        var comment = new CommentModel
        {
            CommentId = 1, PageId = 1, UserId = 5, LikeCount = 3, Content = "Test"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync((CommentLikeModel?)null);
        _commentLikeRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<CommentLikeModel>()))
            .ReturnsAsync((CommentLikeModel cl) => cl);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) => c);

        var result = await _service.ToggleLikeAsync(1, 10);

        result.IsLikedByUser.Should().BeTrue();
        result.LikeCount.Should().Be(4);
        _commentLikeRepoMock.Verify(r => r.CreateAsync(It.IsAny<CommentLikeModel>()), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_RemovesLike_WhenAlreadyLiked()
    {
        var comment = new CommentModel
        {
            CommentId = 1, PageId = 1, UserId = 5, LikeCount = 3, Content = "Test"
        };
        var existingLike = new CommentLikeModel
        {
            CommentLikeId = 50, CommentId = 1, UserId = 10, CreatedAt = DateTime.UtcNow
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
        _commentLikeRepoMock.Setup(r => r.GetAsync(1, 10)).ReturnsAsync(existingLike);
        _commentLikeRepoMock.Setup(r => r.DeleteAsync(existingLike)).Returns(Task.CompletedTask);
        _commentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync((CommentModel c) => c);

        var result = await _service.ToggleLikeAsync(1, 10);

        result.IsLikedByUser.Should().BeFalse();
        result.LikeCount.Should().Be(2);
    }

    [Fact]
    public async Task ToggleLikeAsync_ThrowsKeyNotFoundException_ForNonExistentComment()
    {
        _commentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((CommentModel?)null);

        var act = () => _service.ToggleLikeAsync(999, 10);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}
