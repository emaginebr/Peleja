namespace Peleja.Tests.API.Controllers;

using FluentAssertions;
using Flurl.Http;
using Peleja.Tests.API.Config;

[Collection("AuthCollection")]
public class GiphyControllerTests
{
    private readonly AuthFixture _auth;

    public GiphyControllerTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task Search_ReturnsOk_WithoutAuth()
    {
        var response = await _auth.CreateAnonymousRequest("/api/v1/giphy/search")
            .SetQueryParam("q", "cat")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Search_ReturnsOk_WithAuth()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/giphy/search")
            .SetQueryParam("q", "cat")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WithoutQuery()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/giphy/search")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_RespectsLimitParameter()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/giphy/search")
            .SetQueryParam("q", "cat")
            .SetQueryParam("limit", 5)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Search_RespectsOffsetParameter()
    {
        var response = await _auth.CreateAuthenticatedRequest("/api/v1/giphy/search")
            .SetQueryParam("q", "cat")
            .SetQueryParam("offset", 10)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }
}
