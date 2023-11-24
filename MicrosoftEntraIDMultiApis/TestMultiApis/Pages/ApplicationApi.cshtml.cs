using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorAzureAD.Pages;

[Authorize]
public class ApplicationApiModel : PageModel
{
    private readonly MultiTenantApplicationApiService _apiService;

    public List<string> DataFromApi { get; set; } = new List<string>();

    public ApplicationApiModel(MultiTenantApplicationApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task OnGetAsync()
    {
        DataFromApi = await _apiService.GetApiDataAsync();

        //var mustFail = await _apiService.GetApiDataAsync(true);
    }
}