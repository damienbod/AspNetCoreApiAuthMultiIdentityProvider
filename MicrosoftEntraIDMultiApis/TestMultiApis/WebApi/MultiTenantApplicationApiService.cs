using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace RazorAzureAD;

public class MultiTenantApplicationApiService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _configuration;

    public MultiTenantApplicationApiService(IHttpClientFactory clientFactory,
        IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _configuration = configuration;
    }

    public async Task<List<string>> GetApiDataAsync(bool testIncorrectMultiEndpoint = false)
    {
        // 1. Client client credentials client
        var app = ConfidentialClientApplicationBuilder
            .Create(_configuration["AzureADMultiApi:ClientId"])
            .WithClientSecret(_configuration["AzureADMultiApi:ClientSecret"])
            .WithAuthority(_configuration["AzureADMultiApi:Authority"])
            .Build();

        var scopes = new[] { _configuration["AzureADMultiApi:Scope"] }; // default scope

        // 2. Get access token
        var authResult = await app.AcquireTokenForClient(scopes)
            .ExecuteAsync();

        // 3. Use access token to access token
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(_configuration["AzureADMultiApi:ApiBaseAddress"]);

        client.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        client.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        if (testIncorrectMultiEndpoint)
        {
            response = await client.GetAsync("api/Single"); // must fail
        }
        else
        {
            response = await client.GetAsync("api/Multi");

        }

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<List<string>>(responseContent);

            if (data != null)
                return data;
        }

        throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
    }
}