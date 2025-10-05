using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniUrl.Extensions;
using MiniUrl.Models.Requests.Login;
using MiniUrl.Models.Responses;
using MiniUrl.Models.Responses.Login;
using MiniUrl.Services;

namespace MiniUrl.Controllers;

[ApiController]
[Route("api/login")]
[Produces("application/json")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly IValidator<LoginRequest> _validator;
    private readonly IAuthService _authService;

    public LoginController(
        ILogger<LoginController> logger,
        IValidator<LoginRequest> validator,
        IAuthService authService)
    {
        _logger = logger;
        _validator = validator;
        _authService = authService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToErrorResponse(HttpContext));
        }

        var result = await _authService.Login(request);
        return Ok(result);
    }
}
