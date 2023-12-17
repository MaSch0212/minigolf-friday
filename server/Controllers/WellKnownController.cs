using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MinigolfFriday;

public record GetWellKnownResponse(string FacebookAppId);

[AllowAnonymous]
[Route("api/.well-known")]
public class WellKnownController(IOptionsMonitor<FacebookOptions> facebookOptions) : Controller
{
    private readonly IOptionsMonitor<FacebookOptions> _facebookOptions = facebookOptions;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new GetWellKnownResponse(_facebookOptions.CurrentValue.AppId));
    }
}
