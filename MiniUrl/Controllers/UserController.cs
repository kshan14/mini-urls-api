using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniUrl.Models.Requests.User;
using MiniUrl.Models.Responses;
using MiniUrl.Models.Responses.User;
using MiniUrl.Services;

namespace MiniUrl.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IValidator<CreateUserRequest> _createUserRequestValidator;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IValidator<CreateUserRequest> createUserRequestValidator,
        IUserService userService)
    {
        _logger = logger;
        _createUserRequestValidator = createUserRequestValidator;
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var validationResult = await _createUserRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var user = await _userService.CreateUser(request);
        return Created(string.Empty, user);
    }
}
