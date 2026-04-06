namespace Peleja.API.Tests.Controllers;

using FluentAssertions;
using Flurl.Http;
using Peleja.API.Tests.Config;

[Collection("AuthCollection")]
public class CommentLikeControllerTests
{
    private readonly AuthFixture _auth;
    private readonly TestSettings _settings;

    public CommentLikeControllerTests(AuthFixture auth)
    {
        _auth = auth;
        _settings = auth.Settings;
    }

    [Fact]
    public async Task ToggleLike_Returns401_WithoutAuth()
    {
        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments/1/like"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .AllowAnyHttpStatus()
            .PostAsync();

        // Assert
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ToggleLike_ReturnsOk_WithAuth()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange - first create a comment to like
        var createResponse = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to like"
            });

        if (createResponse.StatusCode != 201)
            return;

        var createBody = await createResponse.GetJsonAsync<dynamic>();
        long commentId = (long)createBody.dados.commentId;

        // Act - toggle like on
        var response = await $"{_settings.BaseUrl}/api/v1/comments/{commentId}/like"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostAsync();

        // Assert
        response.StatusCode.Should().Be(200);
        var body = await response.GetJsonAsync<dynamic>();
        ((bool)body.sucesso).Should().BeTrue();
        ((bool)body.dados.isLikedByUser).Should().BeTrue();
    }

    [Fact]
    public async Task ToggleLike_TogglesOff_WhenAlreadyLiked()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange - create a comment
        var createResponse = await $"{_settings.BaseUrl}/api/v1/comments"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to toggle like"
            });

        if (createResponse.StatusCode != 201)
            return;

        var createBody = await createResponse.GetJsonAsync<dynamic>();
        long commentId = (long)createBody.dados.commentId;

        // Like it first
        await $"{_settings.BaseUrl}/api/v1/comments/{commentId}/like"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .PostAsync();

        // Act - toggle like off
        var response = await $"{_settings.BaseUrl}/api/v1/comments/{commentId}/like"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostAsync();

        // Assert
        response.StatusCode.Should().Be(200);
        var body = await response.GetJsonAsync<dynamic>();
        ((bool)body.dados.isLikedByUser).Should().BeFalse();
    }

    [Fact]
    public async Task ToggleLike_Returns404_ForNonExistentComment()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Act
        var response = await $"{_settings.BaseUrl}/api/v1/comments/999999/like"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .PostAsync();

        // Assert
        response.StatusCode.Should().Be(404);
    }
}
