using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace RazorAzureAD;

public class SingleTenantApiService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public SingleTenantApiService(IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition,
        IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
    }

    public async Task<List<string>> GetApiDataAsync(bool testIncorrectMultiEndpoint = false)
    {
        var client = _clientFactory.CreateClient();

        var scope = _configuration["AzureADSingleApi:ScopeForAccessToken"];
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });

        client.BaseAddress = new Uri(_configuration["AzureADSingleApi:ApiBaseAddress"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        if (testIncorrectMultiEndpoint)
        {
            response = await client.GetAsync("api/Multi"); // must fail
        }
        else
        {
            response = await client.GetAsync("api/Single");
        }


        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<List<string>>(responseContent);

            if(data != null)
                return data;
        }

        throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
    }
}