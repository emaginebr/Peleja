namespace Peleja.Tests.Repositories;

using FluentAssertions;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;
using Peleja.Infra.Repositories;

public class UserRepositoryTests
{
    private async Task<(Peleja.Infra.Context.PelejaContext context, Tenant tenant)> SetupWithTenant()
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
        return (context, tenant);
    }

    [Fact]
    public async Task CreateAsync_PersistsUser()
    {
        var (context, tenant) = await SetupWithTenant();
        var repo = new UserRepository(context);

        var user = new User
        {
            TenantId = tenant.TenantId,
            NauthUserId = "nauth-123",
            DisplayName = "Test User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var result = await repo.CreateAsync(user);

        result.UserId.Should().BeGreaterThan(0);
        result.DisplayName.Should().Be("Test User");
        context.Dispose();
    }

    [Fact]
    public async Task GetByNauthUserIdAsync_ReturnsUser_WhenExists()
    {
        var (context, tenant) = await SetupWithTenant();
        context.Users.Add(new User
        {
            TenantId = tenant.TenantId,
            NauthUserId = "nauth-456",
            DisplayName = "Found User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByNauthUserIdAsync(tenant.TenantId, "nauth-456");

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Found User");
        context.Dispose();
    }

    [Fact]
    public async Task GetByNauthUserIdAsync_ReturnsNull_WhenDifferentTenant()
    {
        var (context, tenant) = await SetupWithTenant();
        context.Users.Add(new User
        {
            TenantId = tenant.TenantId,
            NauthUserId = "nauth-789",
            DisplayName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByNauthUserIdAsync(999, "nauth-789");

        result.Should().BeNull();
        context.Dispose();
    }
}
