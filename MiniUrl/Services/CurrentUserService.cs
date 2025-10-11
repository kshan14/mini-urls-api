using System.Security.Claims;
using MiniUrl.Entities;
using MiniUrl.Exceptions;

namespace MiniUrl.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     throw new UnauthorizedException();
        ;

        return Guid.Parse(userId);
    }

    public string GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    }

    public Role GetUserRole()
    {
        var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ??
                       throw new UnauthorizedException();

        return Enum.Parse<Role>(userRole);
    }

    public bool IsSameRole(Role userRole)
    {
        return GetUserRole().Equals(userRole);
    }
}
