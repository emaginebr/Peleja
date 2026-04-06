namespace Peleja.Tests.API.Config;

using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;

public class AuthFixture : IAsyncLifetime
{
    public string BaseUrl { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;

    private IConfiguration _configuration = null!;
    private string _userAgent = "Peleja.ApiTests/1.0";
    private string _deviceFingerprint = "api-test-device";
    private string _tenant = string.Empty;

    public async Task InitializeAsync()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        BaseUrl = _configuration["ApiBaseUrl"] ?? throw new Exception("ApiBaseUrl not configured");

        var authBaseUrl = _configuration["Auth:BaseUrl"] ?? throw new Exception("Auth:BaseUrl not configured");
        _tenant = _configuration["Auth:Tenant"] ?? throw new Exception("Auth:Tenant not configured");
        _userAgent = _configuration["Auth:UserAgent"] ?? "Peleja.ApiTests/1.0";
        _deviceFingerprint = _configuration["Auth:DeviceFingerprint"] ?? "api-test-device";
        var email = _configuration["Auth:Email"] ?? throw new Exception("Auth:Email not configured");
        var password = _configuration["Auth:Password"] ?? throw new Exception("Auth:Password not configured");
        var loginEndpoint = _configuration["Auth:LoginEndpoint"] ?? "/user/loginWithEmail";

        try
        {
            var response = await new Url(authBaseUrl)
                .AppendPathSegment(loginEndpoint)
                .WithHeader("X-Tenant-Id", _tenant)
                .WithHeader("User-Agent", _userAgent)
                .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
                .WithAutoRedirect(true)
                .PostJsonAsync(new { email, password })
                .ReceiveJson<LoginResponse>();

            AuthToken = response?.Token ?? string.Empty;
        }
        catch (FlurlHttpException ex)
        {
            throw new Exception($"Failed to authenticate for API tests. Status: {ex.StatusCode}. Ensure the Auth API is running at {authBaseUrl}", ex);
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public IFlurlRequest CreateAuthenticatedRequest(string path)
    {
        return new Url(BaseUrl)
            .AppendPathSegment(path)
            .WithOAuthBearerToken(AuthToken)
            .WithHeader("X-Tenant-Id", _tenant)
            .WithHeader("User-Agent", _userAgent)
            .WithHeader("X-Device-Fingerprint", _deviceFingerprint);
    }

    public IFlurlRequest CreateAnonymousRequest(string path)
    {
        return new Url(BaseUrl)
            .AppendPathSegment(path)
            .WithHeader("X-Tenant-Id", _tenant)
            .WithHeader("User-Agent", _userAgent)
            .WithHeader("X-Device-Fingerprint", _deviceFingerprint);
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}

[CollectionDefinition("AuthCollection")]
public class AuthCollection : ICollectionFixture<AuthFixture> { }
