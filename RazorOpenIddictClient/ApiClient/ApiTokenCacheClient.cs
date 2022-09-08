using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;

namespace RazorPageOidcClient;

public class ApiTokenCacheClient
{
    private readonly ILogger<ApiTokenCacheClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private static readonly Object _lock = new();
    private readonly IDistributedCache _cache;

    private const int cacheExpirationInDays = 1;

    private class AccessTokenItem
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
    }

    public ApiTokenCacheClient(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        IDistributedCache cache)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _logger = loggerFactory.CreateLogger<ApiTokenCacheClient>();
        _cache = cache;
    }

    public async Task<string> GetApiToken(string api_name, string api_scope, string secret)
    {
        var accessToken = GetFromCache(api_name);

        if (accessToken != null)
        {
            if (accessToken.ExpiresIn > DateTime.UtcNow)
            {
                return accessToken.AccessToken;
            }
            else
            {
                // remove  => NOT Needed for this cache type
            }
        }

        _logger.LogDebug("GetApiToken new from STS for {api_name}", api_name);

        // add
        var newAccessToken = await GetApiTokenInternal( api_name,  api_scope,  secret);
        AddToCache(api_name, newAccessToken);

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

    private void AddToCache(string key, AccessTokenItem accessTokenItem)
    {
        var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(cacheExpirationInDays));

        lock (_lock)
        {
            _cache.SetString(key, System.Text.Json.JsonSerializer.Serialize(accessTokenItem), options);
        }
    }

    private AccessTokenItem? GetFromCache(string key)
    {
        var item = _cache.GetString(key);
        if (item != null)
        {
            return System.Text.Json.JsonSerializer.Deserialize<AccessTokenItem>(item);
        }

        return null;
    }
}
