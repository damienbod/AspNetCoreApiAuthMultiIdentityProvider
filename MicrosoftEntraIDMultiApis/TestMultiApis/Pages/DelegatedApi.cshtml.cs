using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;

namespace RazorMicrosoftEntraID.Pages;

[AuthorizeForScopes(Scopes = new string[] { "api://b2a09168-54e2-4bc4-af92-a710a64ef1fa/access_as_user" })]
public class DelegatedApiModel : PageModel
{
    private readonly SingleTenantApiService _apiService;

    public List<string> DataFromApi { get; set; } = new List<string>();

    public DelegatedApiModel(SingleTenantApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task OnGetAsync()
    {
        DataFromApi = await _apiService.GetApiDataAsync();

        //var mustFail = await _apiService.GetApiDataAsync(true);
    }
}