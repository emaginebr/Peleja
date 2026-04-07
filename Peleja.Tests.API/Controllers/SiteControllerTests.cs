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
    public async Task ListSites_ReturnsPaginated_WithAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .SetQueryParam("pageSize", 15)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.TryGetProperty("items", out _).Should().BeTrue();
        body.TryGetProperty("hasMore", out _).Should().BeTrue();
        body.TryGetProperty("nextCursor", out _).Should().BeTrue();
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

    [Fact]
    public async Task ListPages_ReturnsOk_WithAuth()
    {
        // Get siteId from listing sites
        var listResponse = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .GetAsync();

        var listJson = await listResponse.GetStringAsync();
        var sites = JsonDocument.Parse(listJson).RootElement.GetProperty("items");

        if (sites.GetArrayLength() == 0)
            return; // No sites to test with

        var siteId = sites[0].GetProperty("siteId").GetInt64();

        var response = await _auth.CreateTenantRequest($"/api/v1/sites/{siteId}/pages")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(200);
        var json = await response.GetStringAsync();
        var body = JsonDocument.Parse(json).RootElement;
        body.TryGetProperty("items", out _).Should().BeTrue();
        body.TryGetProperty("hasMore", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ListPages_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites/1/pages")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ListCommentsByPage_Returns401_WithoutAuth()
    {
        var response = await _auth.CreateTenantRequest("/api/v1/sites/1/pages/1/comments")
            .AllowAnyHttpStatus()
            .GetAsync();

        response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ListCommentsByPage_ReturnsOk_WithAuth()
    {
        // Create a site, then a comment (which auto-creates a page), then list comments by page
        var createSiteResponse = await _auth.CreateTenantRequest("/api/v1/sites")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .PostJsonAsync(new
            {
                siteUrl = $"https://pagetest-{Guid.NewGuid():N}.example.com",
                tenant = "emagine"
            });

        if (createSiteResponse.StatusCode != 201)
            return;

        var siteJson = await createSiteResponse.GetStringAsync();
        var siteId = JsonDocument.Parse(siteJson).RootElement.GetProperty("siteId").GetInt64();
        var clientId = JsonDocument.Parse(siteJson).RootElement.GetProperty("clientId").GetString()!;

        // Create a comment via the public endpoint (auto-creates page)
        await _auth.CreateAuthenticatedRequest("/api/v1/comments")
            .WithHeader("X-Client-Id", clientId)
            .PostJsonAsync(new
            {
                pageUrl = "https://pagetest.example.com/test-page",
                content = "Test comment for page listing"
            });

        // List pages for the site
        var pagesResponse = await _auth.CreateTenantRequest($"/api/v1/sites/{siteId}/pages")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .GetAsync();

        pagesResponse.StatusCode.Should().Be(200);
        var pagesJson = await pagesResponse.GetStringAsync();
        var pages = JsonDocument.Parse(pagesJson).RootElement.GetProperty("items");

        if (pages.GetArrayLength() == 0)
            return;

        var pageId = pages[0].GetProperty("pageId").GetInt64();
        pages[0].GetProperty("commentCount").GetInt32().Should().BeGreaterThan(0);

        // List comments for the page
        var commentsResponse = await _auth.CreateTenantRequest($"/api/v1/sites/{siteId}/pages/{pageId}/comments")
            .WithOAuthBearerToken(_auth.AuthToken)
            .AllowAnyHttpStatus()
            .GetAsync();

        commentsResponse.StatusCode.Should().Be(200);
        var commentsJson = await commentsResponse.GetStringAsync();
        var commentsBody = JsonDocument.Parse(commentsJson).RootElement;
        commentsBody.TryGetProperty("items", out _).Should().BeTrue();
        commentsBody.TryGetProperty("hasMore", out _).Should().BeTrue();
    }
}
