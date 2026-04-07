namespace Peleja.Tests.API.Controllers;

using System.Text.Json;
using FluentAssertions;
using Flurl.Http;
using Peleja.Tests.API.Config;

[Collection("AuthCollection")]
public class SiteControllerTests
{
    private readonly AuthFixture _auth;

    public SiteControllerTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task CreateSite_Returns201_WithAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                siteUrl = $"https://test-{Guid.NewGuid():N}.example.com",
                tenant = "emagine"
            });

        response.StatusCode.Should().Be(201);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.TryGetProperty("clientId", out _).Should().BeTrue();
        body.TryGetProperty("siteUrl", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateSite_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                siteUrl = "https://unauthorized.example.com",
                tenant = "emagine"
            });

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task CreateSite_Returns409_DuplicateUrl()
    {
        var siteUrl = $"https://dup-{Guid.NewGuid():N}.example.com";

        await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .PostJsonAsync(new { siteUrl, tenant = "emagine" });

        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new { siteUrl, tenant = "emagine" });

        response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task ListSites_ReturnsOk_WithAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ListSites_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task UpdateSite_Returns200_ByOwner()
    {
        var createResponse = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                siteUrl = $"https://update-{Guid.NewGuid():N}.example.com",
                tenant = "emagine"
            });

        createResponse.StatusCode.Should().Be(201);

        var createJson = await createResponse.GetStringAsync();
        var siteId = JsonDocument.Parse(createJson).RootElement.GetProperty("siteId").GetInt64();

        var response = await _auth.CreateTenantRequest($"/api/v1/sites/{siteId}")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .PutJsonAsync(new
            {
                siteUrl = $"https://updated-{Guid.NewGuid():N}.example.com"
            });

        response.StatusCode.Should().Be(200);
    }
}
