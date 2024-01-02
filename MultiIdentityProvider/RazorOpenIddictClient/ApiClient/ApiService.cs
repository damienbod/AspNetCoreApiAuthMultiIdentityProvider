using IdentityModel.Client;
using System.Text.Json;

namespace RazorPageOidcClient;

public class ApiService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ApiTokenCacheClient _apiTokenClient;

    public ApiService(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ApiTokenCacheClient apiTokenClient)
    {
        _configuration = configuration;
        _clientFactory = clientFactory;
        _apiTokenClient = apiTokenClient;
    }

    public async Task<List<string>> GetUnsecureApiDataAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient();

            client.BaseAddress = new Uri(_configuration["ProtectedApiUrl"]);
            var response = await client.GetAsync("api/values");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync());

                if (data != null)
                    return data;

                return new List<string>();
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }

    }
    public async Task<List<string>> GetApiDataAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient();

            client.BaseAddress = new Uri(_configuration["ProtectedApiUrl"]);

            var access_token = await _apiTokenClient.GetApiToken(
                "CC",
                "dataEventRecords",
                "cc_secret"
            );

            client.SetBearerToken(access_token);

            var response = await client.GetAsync("api/values");
            if (response.IsSuccessStatusCode)
            {
                var data = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync());

                if (data != null)
                    return data;

                return new List<string>();
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}
