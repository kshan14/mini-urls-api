using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses.MiniUrl;

namespace MiniUrl.Controllers;

[ApiController]
[Route("api/miniurls")]
public class MiniUrlController : ControllerBase
{
    private readonly ILogger<MiniUrlController> _logger;
    private readonly IValidator<CreateMiniUrlRequest> _miniUrlCreateRequestValidator;

    public MiniUrlController(
        ILogger<MiniUrlController> logger,
        IValidator<CreateMiniUrlRequest> miniUrlCreateRequestValidator
    )
    {
        _logger = logger;
        _miniUrlCreateRequestValidator = miniUrlCreateRequestValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateMiniUrlResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMiniUrl([FromBody] CreateMiniUrlRequest request)
    {
        var validationResult = await _miniUrlCreateRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        return Ok();
    }
}
