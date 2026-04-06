namespace Peleja.Tests.Repositories;

using FluentAssertions;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Repositories;

public class CommentRepositoryTests
{
    private async Task<(PelejaContext context, Page page)> SetupWithPage()
    {
        var context = TestDbContextFactory.Create();
        var page = new Page
        {
            UserId = 1,
            PageUrl = "https://example.com/page",
            CreatedAt = DateTime.UtcNow
        };
        context.Pages.Add(page);
        await context.SaveChangesAsync();
        return (context, page);
    }

    [Fact]
    public async Task CreateAsync_PersistsComment()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        var repo = new CommentRepository(context, mapper);

        var comment = CommentModel.Create(page.PageId, 1, "Hello World", null, null);

        var result = await repo.CreateAsync(comment);

        result.CommentId.Should().BeGreaterThan(0);
        result.Content.Should().Be("Hello World");
        context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsComment()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        context.Comments.Add(new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            Content = "Test comment",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var result = await repo.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Content.Should().Be("Test comment");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageIdAsync_ReturnsOnlyRootComments()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        var root = new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            Content = "Root comment",
            CreatedAt = DateTime.UtcNow
        };
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        context.Comments.Add(new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            ParentCommentId = root.CommentId,
            Content = "Reply comment",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var result = await repo.GetByPageIdAsync(page.PageId, "recent", null, 15);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Root comment");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageIdAsync_ExcludesSoftDeletedComments()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        context.Comments.Add(new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            Content = "Visible comment",
            CreatedAt = DateTime.UtcNow
        });
        context.Comments.Add(new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            Content = "Deleted comment",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var result = await repo.GetByPageIdAsync(page.PageId, "recent", null, 15);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Visible comment");
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageIdAsync_ReturnsInDescendingOrder_ForRecent()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        for (int i = 1; i <= 3; i++)
        {
            context.Comments.Add(new Infra.Context.Comment
            {
                PageId = page.PageId,
                UserId = 1,
                Content = $"Comment {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var result = await repo.GetByPageIdAsync(page.PageId, "recent", null, 15);

        result.Should().HaveCount(3);
        result[0].CommentId.Should().BeGreaterThan(result[1].CommentId);
        result[1].CommentId.Should().BeGreaterThan(result[2].CommentId);
        context.Dispose();
    }

    [Fact]
    public async Task GetByPageIdAsync_RespectsPageSize()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        for (int i = 0; i < 5; i++)
        {
            context.Comments.Add(new Infra.Context.Comment
            {
                PageId = page.PageId,
                UserId = 1,
                Content = $"Comment {i}",
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var result = await repo.GetByPageIdAsync(page.PageId, "recent", null, 2);

        result.Should().HaveCount(3);
        context.Dispose();
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var (context, page) = await SetupWithPage();
        var mapper = TestDbContextFactory.CreateMapper();
        context.Comments.Add(new Infra.Context.Comment
        {
            PageId = page.PageId,
            UserId = 1,
            Content = "Original",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new CommentRepository(context, mapper);

        var comment = (await repo.GetByIdAsync(1))!;
        comment.Update("Updated", null);
        await repo.UpdateAsync(comment);

        var updated = await repo.GetByIdAsync(1);
        updated!.Content.Should().Be("Updated");
        updated.IsEdited.Should().BeTrue();
        context.Dispose();
    }
}
