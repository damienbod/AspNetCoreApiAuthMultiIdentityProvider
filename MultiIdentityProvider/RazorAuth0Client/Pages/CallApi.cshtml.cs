using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorAuth0Client.Pages;

public class CallApiModel : PageModel
{
    private readonly WebApiClient _apiService;

    public List<string> DataFromApi { get; set; } = new List<string>();

    public CallApiModel(WebApiClient apiService)
    {
        _apiService = apiService;
    }

    public async Task OnGetAsync()
    {
        DataFromApi = await _apiService.GetWebApiValuesData();
    }
}