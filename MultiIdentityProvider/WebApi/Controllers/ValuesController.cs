using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(AuthenticationSchemes = Consts.ALL_MY_SCHEMES, Policy = Consts.MY_POLICY_ALL_IDP)]
[Route("api/[controller]")]
public class ValuesController : Controller
{
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "data 1 from the api", "data 2 from the api" };
    }
}
