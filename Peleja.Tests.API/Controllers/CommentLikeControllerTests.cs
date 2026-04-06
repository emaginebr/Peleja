namespace Peleja.Tests.API.Controllers;

using System.Text.Json;
using FluentAssertions;
using Flurl.Http;
using Peleja.Tests.API.Config;

[Collection("AuthCollection")]
public class CommentLikeControllerTests
{
    private readonly AuthFixture _auth;

    public CommentLikeControllerTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task ToggleLike_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateAnonymousRequest("/api/v1/comments/1/like")
            .AllowAnyHttpStatus()
            .PostAsync();

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ToggleLike_ReturnsOk_WithAuth()
    {
        var createResponse = await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to like"
            });

        createResponse.StatusCode.Should().Be(201);

        var createJson = await createResponse.GetStringAsync();
        var commentId = JsonDocument.Parse(createJson).RootElement.GetProperty("commentId").GetInt64();

        var response = await _auth.CreateAuthenticatedRequest($"/api/v1/comments/{commentId}/like")
            .AllowAnyHttpStatus()
            .PostAsync();

        response.StatusCode.Should().Be(200);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.GetProperty("isLikedByUser").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ToggleLike_TogglesOff_WhenAlreadyLiked()
    {
        var createResponse = await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to toggle like"
            });

        createResponse.StatusCode.Should().Be(201);

        var createJson = await createResponse.GetStringAsync();
        var commentId = JsonDocument.Parse(createJson).RootElement.GetProperty("commentId").GetInt64();

        await _auth.CreateAuthenticatedRequest($"/api/v1/comments/{commentId}/like")
            .PostAsync();

        var response = await _auth.CreateAuthenticatedRequest($"/api/v1/comments/{commentId}/like")
            .AllowAnyHttpStatus()
            .PostAsync();

        response.StatusCode.Should().Be(200);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.GetProperty("isLikedByUser").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task ToggleLike_Returns404_ForNonExistentComment()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/comments/999999/like")
            .AllowAnyHttpStatus()
            .PostAsync();

        response.StatusCode.Should().Be(404);
    }
}
