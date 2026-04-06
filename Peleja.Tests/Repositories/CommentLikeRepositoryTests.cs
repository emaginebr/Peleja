namespace Peleja.Tests.Repositories;

using FluentAssertions;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;
using Peleja.Infra.Repositories;

public class CommentLikeRepositoryTests
{
    private async Task<(Peleja.Infra.Context.PelejaContext context, Comment comment, User user)> Setup()
    {
        var context = TestDbContextFactory.Create();
        var tenant = new Tenant
        {
            Name = "Test",
            Slug = "test",
            NauthApiUrl = "https://nauth.example.com",
            NauthApiKey = "key",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var user = new User
        {
            TenantId = tenant.TenantId,
            NauthUserId = "nauth-1",
            DisplayName = "Test User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var comment = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Test",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        return (context, comment, user);
    }

    [Fact]
    public async Task CreateAsync_PersistsLike()
    {
        var (context, comment, user) = await Setup();
        var repo = new CommentLikeRepository(context);

        var like = new CommentLike
        {
            CommentId = comment.CommentId,
            UserId = user.UserId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await repo.CreateAsync(like);

        result.CommentLikeId.Should().BeGreaterThan(0);
        context.Dispose();
    }

    [Fact]
    public async Task GetAsync_ReturnsLike_WhenExists()
    {
        var (context, comment, user) = await Setup();
        context.CommentLikes.Add(new CommentLike
        {
            CommentId = comment.CommentId,
            UserId = user.UserId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentLikeRepository(context);

        var result = await repo.GetAsync(comment.CommentId, user.UserId);

        result.Should().NotBeNull();
        context.Dispose();
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNotExists()
    {
        var (context, comment, user) = await Setup();
        var repo = new CommentLikeRepository(context);

        var result = await repo.GetAsync(comment.CommentId, user.UserId);

        result.Should().BeNull();
        context.Dispose();
    }

    [Fact]
    public async Task DeleteAsync_RemovesLike()
    {
        var (context, comment, user) = await Setup();
        var like = new CommentLike
        {
            CommentId = comment.CommentId,
            UserId = user.UserId,
            CreatedAt = DateTime.UtcNow
        };
        context.CommentLikes.Add(like);
        await context.SaveChangesAsync();

        var repo = new CommentLikeRepository(context);

        await repo.DeleteAsync(like);

        var result = await repo.GetAsync(comment.CommentId, user.UserId);
        result.Should().BeNull();
        context.Dispose();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenLikeExists()
    {
        var (context, comment, user) = await Setup();
        context.CommentLikes.Add(new CommentLike
        {
            CommentId = comment.CommentId,
            UserId = user.UserId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentLikeRepository(context);

        var result = await repo.ExistsAsync(comment.CommentId, user.UserId);

        result.Should().BeTrue();
        context.Dispose();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenNoLike()
    {
        var (context, comment, user) = await Setup();
        var repo = new CommentLikeRepository(context);

        var result = await repo.ExistsAsync(comment.CommentId, user.UserId);

        result.Should().BeFalse();
        context.Dispose();
    }
}
