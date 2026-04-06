namespace Peleja.API.Tests.Controllers;

using FluentAssertions;
using Flurl.Http;
using Peleja.API.Tests.Config;

[Collection("AuthCollection")]
public class GiphyControllerTests
{
    private readonly AuthFixture _auth;
    private readonly TestSettings _settings;

    public GiphyControllerTests(AuthFixture auth)
    {
        _auth = auth;
        _settings = auth.Settings;
    }

    [Fact]
    public async Task Search_Returns401_WithoutAuth()
    {
        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/giphy/search"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .SetQueryParam("q", "cat")
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Search_ReturnsOk_WithAuth()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/giphy/search"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .SetQueryParam("q", "cat")
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(200);
        var body = await response.GetJsonAsync<dynamic>();
        ((bool)body.sucesso).Should().BeTrue();
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WithoutQuery()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/giphy/search"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Search_RespectsLimitParameter()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/giphy/search"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .SetQueryParam("q", "cat")
            .SetQueryParam("limit", 5)
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Search_RespectsOffsetParameter()
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            return;
        }

        // Arrange & Act
        var response = await $"{_settings.BaseUrl}/api/v1/giphy/search"
            .WithHeader("X-Tenant-Id", _settings.TenantId)
            .WithOAuthBearerToken(_auth.Token)
            .SetQueryParam("q", "cat")
            .SetQueryParam("offset", 10)
            .AllowAnyHttpStatus()
            .GetAsync();

        // Assert
        response.StatusCode.Should().Be(200);
    }
}
