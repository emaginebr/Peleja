namespace Peleja.Domain.Models;

public class Tenant
{
    public long TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string NauthApiUrl { get; set; } = string.Empty;
    public string NauthApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
