namespace Peleja.API.Tests.Config;

using Flurl.Http;
using Microsoft.Extensions.Configuration;

public class AuthFixture : IAsyncLifetime
{
    public string Token { get; private set; } = string.Empty;
    public TestSettings Settings { get; private set; } = new();

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Settings = configuration.GetSection("TestSettings").Get<TestSettings>() ?? new TestSettings();

        try
        {
            // Attempt NAuth login to get token
            var response = await $"{Settings.BaseUrl}/api/auth/login"
                .WithHeader("X-Tenant-Id", Settings.TenantId)
                .PostJsonAsync(new { username = Settings.Username, password = Settings.Password });

            var result = await response.GetJsonAsync<dynamic>();
            Token = result?.token ?? string.Empty;
        }
        catch
        {
            // If auth service is unavailable, tests requiring auth will be skipped
            Token = string.Empty;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("AuthCollection")]
public class AuthCollection : ICollectionFixture<AuthFixture> { }
