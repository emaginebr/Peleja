namespace Peleja.Tests.Infra.Repositories;

using FluentAssertions;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Repositories;
using Peleja.Tests.Infra.Context;

public class CommentLikeRepositoryTests
{
    private async Task<(PelejaContext context, Comment comment)> Setup()
    {
        var context = TestDbContextFactory.Create();
        var page = new Page { SiteId = 1, PageUrl = "https://example.com/page", CreatedAt = DateTime.Now };
        context.Pages.Add(page);
        await context.SaveChangesAsync();
        var comment = new Comment { PageId = page.PageId, UserId = 1, Content = "Test", CreatedAt = DateTime.Now };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        return (context, comment);
    }

    [Fact]
    public async Task CreateAsync_PersistsLike()
    {
        var (context, comment) = await Setup();
        var mapper = TestDbContextFactory.CreateMapper();
        var repo = new CommentLikeRepository(context, mapper);
        var like = CommentLikeModel.Create(comment.CommentId, 10);
        var result = await repo.CreateAsync(like);
        result.CommentLikeId.Should().BeGreaterThan(0);
        context.Dispose();
    }

    [Fact]
    public async Task GetAsync_ReturnsLike_WhenExists()
    {
        var (context, comment) = await Setup();
        var mapper = TestDbContextFactory.CreateMapper();
        context.CommentLikes.Add(new CommentLike { CommentId = comment.CommentId, UserId = 10, CreatedAt = DateTime.Now });
        await context.SaveChangesAsync();
        var repo = new CommentLikeRepository(context, mapper);
        var result = await repo.GetAsync(comment.CommentId, 10);
        result.Should().NotBeNull();
        context.Dispose();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenNoLike()
    {
        var (context, comment) = await Setup();
        var mapper = TestDbContextFactory.CreateMapper();
        var repo = new CommentLikeRepository(context, mapper);
        var result = await repo.ExistsAsync(comment.CommentId, 10);
        result.Should().BeFalse();
        context.Dispose();
    }

    [Fact]
    public async Task DeleteAsync_RemovesLike()
    {
        var (context, comment) = await Setup();
        var mapper = TestDbContextFactory.CreateMapper();
        context.CommentLikes.Add(new CommentLike { CommentId = comment.CommentId, UserId = 10, CreatedAt = DateTime.Now });
        await context.SaveChangesAsync();
        var repo = new CommentLikeRepository(context, mapper);
        var like = (await repo.GetAsync(comment.CommentId, 10))!;
        await repo.DeleteAsync(like);
        var result = await repo.GetAsync(comment.CommentId, 10);
        result.Should().BeNull();
        context.Dispose();
    }
}
