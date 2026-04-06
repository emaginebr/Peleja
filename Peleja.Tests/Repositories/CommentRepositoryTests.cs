namespace Peleja.Tests.Repositories;

using FluentAssertions;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;
using Peleja.Infra.Repositories;

public class CommentRepositoryTests
{
    private async Task<(Peleja.Infra.Context.PelejaContext context, Tenant tenant, User user)> SetupWithTenantAndUser()
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

        return (context, tenant, user);
    }

    [Fact]
    public async Task CreateAsync_PersistsComment()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        var repo = new CommentRepository(context);

        var comment = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Hello World",
            CreatedAt = DateTime.UtcNow
        };

        var result = await repo.CreateAsync(comment);

        result.CommentId.Should().BeGreaterThan(0);
        result.Content.Should().Be("Hello World");
        context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsComment_WithUser()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        var comment = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Test comment",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        var result = await repo.GetByIdAsync(comment.CommentId);

        result.Should().NotBeNull();
        result!.Content.Should().Be("Test comment");
        result.User.Should().NotBeNull();
        result.User!.DisplayName.Should().Be("Test User");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsOnlyRootComments()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        var root = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Root comment",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        var reply = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            ParentCommentId = root.CommentId,
            PageUrl = "https://example.com/page",
            Content = "Reply comment",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(reply);
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        var result = await repo.GetByPageUrlAsync(
            tenant.TenantId, "https://example.com/page", "recent", null, 15);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Root comment");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ExcludesSoftDeletedComments()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        context.Comments.Add(new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Visible comment",
            CreatedAt = DateTime.UtcNow
        });
        context.Comments.Add(new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Deleted comment",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        var result = await repo.GetByPageUrlAsync(
            tenant.TenantId, "https://example.com/page", "recent", null, 15);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Visible comment");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageUrlAsync_ReturnsInDescendingOrder_ForRecent()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        for (int i = 1; i <= 3; i++)
        {
            context.Comments.Add(new Comment
            {
                TenantId = tenant.TenantId,
                UserId = user.UserId,
                PageUrl = "https://example.com/page",
                Content = $"Comment {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        var result = await repo.GetByPageUrlAsync(
            tenant.TenantId, "https://example.com/page", "recent", null, 15);

        result.Should().HaveCount(3);
        result[0].CommentId.Should().BeGreaterThan(result[1].CommentId);
        result[1].CommentId.Should().BeGreaterThan(result[2].CommentId);
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageUrlAsync_RespectsPageSize()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        for (int i = 0; i < 5; i++)
        {
            context.Comments.Add(new Comment
            {
                TenantId = tenant.TenantId,
                UserId = user.UserId,
                PageUrl = "https://example.com/page",
                Content = $"Comment {i}",
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        // pageSize=2, repo returns pageSize+1 = 3 to signal hasMore
        var result = await repo.GetByPageUrlAsync(
            tenant.TenantId, "https://example.com/page", "recent", null, 2);

        result.Should().HaveCount(3);
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageUrlAsync_IsolatesByTenant()
    {
        var (context, tenant1, user) = await SetupWithTenantAndUser();
        var tenant2 = new Tenant
        {
            Name = "Other",
            Slug = "other",
            NauthApiUrl = "https://nauth.example.com",
            NauthApiKey = "key2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant2);
        await context.SaveChangesAsync();

        context.Comments.Add(new Comment
        {
            TenantId = tenant1.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Tenant 1 comment",
            CreatedAt = DateTime.UtcNow
        });
        context.Comments.Add(new Comment
        {
            TenantId = tenant2.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Tenant 2 comment",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        var result = await repo.GetByPageUrlAsync(
            tenant1.TenantId, "https://example.com/page", "recent", null, 15);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Tenant 1 comment");
        context.Dispose();
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var (context, tenant, user) = await SetupWithTenantAndUser();
        var comment = new Comment
        {
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            PageUrl = "https://example.com/page",
            Content = "Original",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context);

        comment.Content = "Updated";
        comment.IsEdited = true;
        await repo.UpdateAsync(comment);

        var updated = await repo.GetByIdAsync(comment.CommentId);
        updated!.Content.Should().Be("Updated");
        updated.IsEdited.Should().BeTrue();
        context.Dispose();
    }
}
