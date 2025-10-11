using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniUrl.Extensions;
using MiniUrl.Models.Requests.Common;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses;
using MiniUrl.Models.Responses.Common;
using MiniUrl.Models.Responses.MiniUrl;
using MiniUrl.Services;

namespace MiniUrl.Controllers;

[ApiController]
[Route("api/miniurls")]
[Produces("application/json")]
public class MiniUrlController : ControllerBase
{
    private readonly ILogger<MiniUrlController> _logger;
    private readonly IValidator<PaginationRequest> _paginationRequestValidator;
    private readonly IValidator<CreateMiniUrlRequest> _miniUrlCreateRequestValidator;
    private readonly IMiniUrlViewService _miniUrlViewService;
    private readonly IMiniUrlGenerator _miniUrlGenerator;

    public MiniUrlController(
        ILogger<MiniUrlController> logger,
        IValidator<PaginationRequest> paginationRequestValidator,
        IValidator<CreateMiniUrlRequest> miniUrlCreateRequestValidator,
        IMiniUrlViewService miniUrlViewService,
        IMiniUrlGenerator miniUrlGenerator
    )
    {
        _logger = logger;
        _paginationRequestValidator = paginationRequestValidator;
        _miniUrlCreateRequestValidator = miniUrlCreateRequestValidator;
        _miniUrlViewService = miniUrlViewService;
        _miniUrlGenerator = miniUrlGenerator;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<GetTinyUrlResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromQuery] PaginationRequest request)
    {
        var validationResult = await _paginationRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        return Ok(await _miniUrlViewService.GetUrls(request));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    [ProducesResponseType(typeof(CreateMiniUrlResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMiniUrl([FromBody] CreateMiniUrlRequest request)
    {
        var validationResult = await _miniUrlCreateRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToErrorResponse(HttpContext));
        }

        var result = await _miniUrlGenerator.GenerateUrl(request).ConfigureAwait(false);
        return Created(string.Empty, result);
    }

    [Authorize(Roles = "Admin")]
    [Route("approve/{id:guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        await _miniUrlGenerator.ApproveUrl(id);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [Route("deny/{id:guid}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deny([FromRoute] Guid id)
    {
        await _miniUrlGenerator.DenyUrl(id);
        return NoContent();
    }
    
}