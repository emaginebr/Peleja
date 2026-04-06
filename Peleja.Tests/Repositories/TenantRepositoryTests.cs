namespace Peleja.Tests.Repositories;

using FluentAssertions;
using Peleja.Domain.Models;
using Peleja.Infra.Repositories;

public class TenantRepositoryTests
{
    [Fact]
    public async Task GetBySlugAsync_ReturnsTenant_WhenExists()
    {
        using var context = TestDbContextFactory.Create();
        var tenant = new Tenant
        {
            Name = "Test Site",
            Slug = "test-site",
            NauthApiUrl = "https://nauth.example.com",
            NauthApiKey = "key123",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var repo = new TenantRepository(context);

        var result = await repo.GetBySlugAsync("test-site");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Site");
        result.Slug.Should().Be("test-site");
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsNull_WhenNotExists()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new TenantRepository(context);

        var result = await repo.GetBySlugAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTenant_WhenExists()
    {
        using var context = TestDbContextFactory.Create();
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

        var repo = new TenantRepository(context);

        var result = await repo.GetByIdAsync(tenant.TenantId);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be(tenant.TenantId);
    }
}
