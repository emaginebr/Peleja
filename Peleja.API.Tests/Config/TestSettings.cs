namespace Peleja.API.Tests.Config;

public class TestSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string TenantId { get; set; } = "test-tenant";
    public string Username { get; set; } = "testuser";
    public string Password { get; set; } = "testpassword";
}
