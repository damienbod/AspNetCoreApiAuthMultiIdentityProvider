using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RazorPageOidcClient.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApiService _apiService;

        public List<string> Data = new List<string>();
        public IndexModel(ILogger<IndexModel> logger, ApiService apiService)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            //var result = await _apiService.GetUnsecureApiDataAsync();
            Data = await _apiService.GetApiDataAsync();

            Console.WriteLine(Data.FirstOrDefault());
        }
    }
}
