using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MinigolfFriday;

public record GetWellKnownResponse(string? FacebookAppId);

[Route("api/.well-known")]
public class WellKnownController(IOptionsMonitor<FacebookSignedRequestOptions> facebookOptions)
    : Controller
{
    private readonly IOptionsMonitor<FacebookSignedRequestOptions> _facebookOptions =
        facebookOptions;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new GetWellKnownResponse(_facebookOptions.CurrentValue.AppId));
    }
}
