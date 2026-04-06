namespace Peleja.Tests.API.Controllers;

using System.Text.Json;
using FluentAssertions;
using Flurl.Http;
using Peleja.Tests.API.Config;

[Collection("AuthCollection")]
public class CommentControllerTests
{
    private readonly AuthFixture _auth;

    public CommentControllerTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task GetComments_ReturnsOk_WithoutAuth()
    {
        var response = await _auth.CreateAnonymousRequest("/api/v1/comments")
            .SetQueryParam("pageUrl", "https://example.com/test-page")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetComments_ReturnsPaginated_WithCursor()
    {
        var response = await _auth.CreateAnonymousRequest("/api/v1/comments")
            .SetQueryParam("pageUrl", "https://example.com/test-page")
            .SetQueryParam("sortBy", "recent")
            .SetQueryParam("pageSize", 5)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.TryGetProperty("items", out _).Should().BeTrue();
        body.TryGetProperty("hasMore", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateComment_Returns201_WithAuth()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Integration test comment"
            });

        response.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateComment_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateAnonymousRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Unauthorized comment"
            });

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task UpdateComment_Returns200_ByAuthor()
    {
        var createResponse = await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to update"
            });

        createResponse.StatusCode.Should().Be(201);

        var createJson = await createResponse.GetStringAsync();
        var commentId = JsonDocument.Parse(createJson).RootElement.GetProperty("commentId").GetInt64();

        var response = await _auth.CreateAuthenticatedRequest($"/api/v1/comments/{commentId}")
            .AllowAnyHttpStatus()
            .PutJsonAsync(new
            {
                content = "Updated comment content"
            });

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteComment_Returns204_ByAuthor()
    {
        var createResponse = await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                pageUrl = "https://example.com/test-page",
                content = "Comment to delete"
            });

        createResponse.StatusCode.Should().Be(201);

        var createJson = await createResponse.GetStringAsync();
        var commentId = JsonDocument.Parse(createJson).RootElement.GetProperty("commentId").GetInt64();

        var response = await _auth.CreateAuthenticatedRequest($"/api/v1/comments/{commentId}")
            .AllowAnyHttpStatus()
            .DeleteAsync();

        response.StatusCode.Should().Be(204);
    }
}
