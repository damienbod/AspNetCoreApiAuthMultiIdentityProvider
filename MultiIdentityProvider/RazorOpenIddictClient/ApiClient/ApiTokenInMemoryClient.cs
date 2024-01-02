using IdentityModel.Client;
using System.Collections.Concurrent;

namespace RazorPageOidcClient;

public class ApiTokenInMemoryClient
{
    private readonly ILogger<ApiTokenInMemoryClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private class AccessTokenItem
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
    }

    private readonly ConcurrentDictionary<string, AccessTokenItem> _accessTokens = new();

    public ApiTokenInMemoryClient(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _logger = loggerFactory.CreateLogger<ApiTokenInMemoryClient>();
    }

    public async Task<string> GetApiToken(string api_name, string api_scope, string secret)
    {
        if (_accessTokens.ContainsKey(api_name))
        {
            var accessToken = _accessTokens.GetValueOrDefault(api_name);
            if (accessToken!.ExpiresIn > DateTime.UtcNow)
            {
                return accessToken.AccessToken;
            }
            else
            {
                // remove
                _accessTokens.TryRemove(api_name, out _);
            }
        }

        _logger.LogDebug("GetApiToken new from STS for {api_name}", api_name);

        // add
        var newAccessToken = await GetApiTokenInternal(api_name, api_scope, secret);
        _accessTokens.TryAdd(api_name, newAccessToken);

        return newAccessToken.AccessToken;
    }

    private async Task<AccessTokenItem> GetApiTokenInternal(string api_name, string api_scope, string secret)
    {
        try
        {
            var disco = await HttpClientDiscoveryExtensions.GetDiscoveryDocumentAsync(
                _httpClient,
                _configuration["OpenIDConnectSettings:Authority"]);

            if (disco.IsError)
            {
                _logger.LogError("disco error Status code: {IsError}, Error: {Error}", disco.IsError, disco.Error);
                throw new ApplicationException($"Status code: {disco.IsError}, Error: {disco.Error}");
            }

            var tokenResponse = await HttpClientTokenRequestExtensions.RequestClientCredentialsTokenAsync(_httpClient, new ClientCredentialsTokenRequest
            {
                Scope = api_scope,
                ClientSecret = secret,
                Address = disco.TokenEndpoint,
                ClientId = api_name
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError("tokenResponse.IsError Status code: {IsError}, Error: {Error}", tokenResponse.IsError, tokenResponse.Error);
                throw new ApplicationException($"Status code: {tokenResponse.IsError}, Error: {tokenResponse.Error}");
            }

            return new AccessTokenItem
            {
                ExpiresIn = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                AccessToken = tokenResponse.AccessToken
            };

        }
        catch (Exception e)
        {
            _logger.LogError("Exception {Exception}", e);
            throw new ApplicationException($"Exception {e}");
        }
    }
}
