using System.Text.Json.Serialization;

namespace RazorAuth0Client;

public class Auth0ClientCrendentials
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("audience")]
    public string Audience { get; set; } = string.Empty;

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "client_credentials";
}