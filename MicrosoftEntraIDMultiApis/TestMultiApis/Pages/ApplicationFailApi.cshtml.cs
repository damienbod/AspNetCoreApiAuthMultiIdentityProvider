using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorMicrosoftEntraID.Pages;

[Authorize]
public class ApplicationFailApiModel : PageModel
{
    private readonly MultiTenantApplicationApiService _apiService;

    public List<string> DataFromApi { get; set; } = new List<string>();

    public ApplicationFailApiModel(MultiTenantApplicationApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task OnGetAsync()
    {
        // Must fail
        DataFromApi = await _apiService.GetApiDataAsync(true);
    }
}