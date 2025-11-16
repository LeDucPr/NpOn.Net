using Microsoft.AspNetCore.Mvc;

namespace SSO.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Test controller is working!");
    }
}