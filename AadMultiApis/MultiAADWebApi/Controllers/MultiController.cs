using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(AuthenticationSchemes = "multitenantapi")]
[Route("api/[controller]")]
public class MultiController : Controller
{
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "data 1 from the multi api", "data 2 from multi api" };
    }
}
