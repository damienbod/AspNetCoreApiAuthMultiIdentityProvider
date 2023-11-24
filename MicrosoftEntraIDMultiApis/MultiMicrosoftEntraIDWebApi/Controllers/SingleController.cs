using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(AuthenticationSchemes = Consts.MICROSOFT_ENTRA_ID_SINGLE_SCHEME, Policy = Consts.SINGLE_MICROSOFT_ENTRA_ID_POLICY)]
[Route("api/[controller]")]
public class SingleController : Controller
{
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "data 1 from the single tenant api", "data 2 from single api" };
    }
}
