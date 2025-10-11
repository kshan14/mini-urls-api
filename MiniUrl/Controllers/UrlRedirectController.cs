using Microsoft.AspNetCore.Mvc;
using MiniUrl.Services;

namespace MiniUrl.Controllers;

[ApiController]
[Route("/")]
public class UrlRedirectController : ControllerBase
{
    private readonly ILogger<UrlRedirectController> _logger;
    private IMiniUrlViewService  _viewService;
    
    public UrlRedirectController(ILogger<UrlRedirectController> logger, IMiniUrlViewService viewService)
    {
        _logger = logger;
        _viewService = viewService;
    }

    [HttpGet]
    [Route("{shortenedPath}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string shortenedPath)
    {
        var redirectUrl = await _viewService.GetUrl(shortenedPath);
        return Redirect(redirectUrl);
    }
}