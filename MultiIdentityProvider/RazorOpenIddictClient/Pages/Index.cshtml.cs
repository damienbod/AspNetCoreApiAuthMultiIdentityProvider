using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPageOidcClient.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApiService _apiService;

    public List<string> Data = new();
    public IndexModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task OnGetAsync()
    {
        //var result = await _apiService.GetUnsecureApiDataAsync();
        Data = await _apiService.GetApiDataAsync();

        Console.WriteLine(Data.FirstOrDefault());
    }
}
