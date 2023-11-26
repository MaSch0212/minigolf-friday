using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class MyController : Controller
{
    [HttpGet]
    [Route("api/my")]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello from MyController" });
    }
}
