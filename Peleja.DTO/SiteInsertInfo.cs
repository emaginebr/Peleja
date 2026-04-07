namespace Peleja.DTO;

using System.Text.Json.Serialization;

public class SiteInsertInfo
{
    [JsonPropertyName("siteUrl")]
    public string SiteUrl { get; set; } = string.Empty;

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;
}
