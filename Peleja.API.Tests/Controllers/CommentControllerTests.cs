namespace Peleja.API.Tests.Controllers;

using FluentAssertions;
using Flurl.Http;
using Peleja.API.Tests.Config;

[Collection("AuthCollection")]
public class CommentControllerTests
{
    private readonly AuthFixture _auth;
    private readonly TestSettings _settings;

    public CommentControllerTests(AuthFixture auth)
    {
        _auth = auth;
        _settings = auth.Settings;
    }

    [Fact]
    public async Task GetComments_ReturnsOk_WithoutAuth()
    {
        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .SetQueryParam("pageUrl", "https://example.com/test-page")
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetComments_ReturnsPaginated_WithCursor()
    {
        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .SetQueryParam("pageUrl", "https://example.com/test-page")
            .SetQueryParam("sortBy", "recent")
            .SetQueryParam("pageSize", 5)
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(200);
        var body = await response.GetJsonAsync<dynamic>();
        ((bool)body.sucesso).Should().BeTrue();
    }

    [Fact]
    public async Task CreateComment_Returns201_WithAuth()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            // Skip test if auth is not available
            return;
        }

        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Integration test comment"
            });

        // Assert
        response.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateComment_Returns401_WithoutAuth()
    {
        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Unauthorized comment"
            });

        // Assert
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task UpdateComment_Returns200_ByAuthor()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange - first create a comment
        var createResponse = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to update"
            });

        if (createResponse.StatusCode != 201)
            return;

        var createBody = await createResponse.GetJsonAsync<dynamic>();
        long commentId = (long)createBody.dados.commentId;

        // Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments/{commentId}"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PutJsonAsync(new
            {
                content = "Updated comment content"
            });

        // Assert
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteComment_Returns200_ByAuthor()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange - first create a comment
        var createResponse = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to delete"
            });

        if (createResponse.StatusCode != 201)
            return;

        var createBody = await createResponse.GetJsonAsync<dynamic>();
        long commentId = (long)createBody.dados.commentId;

        // Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments/{commentId}"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .DeleteAsync();

        // Assert
        response.StatusCode.Should().Be(200);
    }
}
