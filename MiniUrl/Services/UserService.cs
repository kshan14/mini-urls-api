using Microsoft.EntityFrameworkCore;
using MiniUrl.Data;
using MiniUrl.Entities;
using MiniUrl.Exceptions;
using MiniUrl.Models.Requests.User;
using MiniUrl.Models.Responses.User;

namespace MiniUrl.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly AppDbContext _dbContext;

    public UserService(ILogger<UserService> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetUserResponse> CreateUser(CreateUserRequest request)
    {
        try
        {
            // 1. Create User Entity and Hash the password
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                Role = Role.User
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User with email: {Email} created", user.Email);
            return new GetUserResponse()
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
        catch (DbUpdateException ex) when (Commons.Utilities.IsUniqueConstraintViolation(ex))
        {
            _logger.LogError(ex, "email or username already exists");
            throw new BadRequestException(
                "email or username already exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error creating user");
            throw new InternalServerException();
        }
    }
}
