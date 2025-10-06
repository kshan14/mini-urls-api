using Microsoft.EntityFrameworkCore;
using MiniUrl.Data;
using MiniUrl.Exceptions;
using MiniUrl.Models.Requests.Login;
using MiniUrl.Models.Responses.Login;

namespace MiniUrl.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;

    public AuthService(
        ILogger<AuthService> logger,
        ITokenService tokenService,
        AppDbContext dbContext)
    {
        _logger = logger;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username.Equals(loginRequest.Username));
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
        {
            _logger.LogWarning("Invalid username or password");
            throw new BadRequestException("Invalid credentials");
        }

        _logger.LogInformation("User with email {Email} logged in", user.Email);
        var token = _tokenService.CreateToken(user);
        return new LoginResponse
        {
            Id = user.Id,
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString(),
        };
    }
}
