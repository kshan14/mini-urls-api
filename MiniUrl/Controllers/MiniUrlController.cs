using Microsoft.AspNetCore.Mvc;

namespace MiniUrl.Controllers;

[ApiController]
public class MiniUrlController(ILogger<MiniUrlController> logger) : ControllerBase
{

    [HttpGet("/hello-world", Name = "GetSmth")]
    public async Task<IActionResult> HelloWorld()
    {
        logger.LogInformation("Hello World");
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        return Ok("Hello World");
    }
}