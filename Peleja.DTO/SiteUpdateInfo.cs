namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class SiteUpdateInfo
{
    [JsonPropertyName("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }
}
