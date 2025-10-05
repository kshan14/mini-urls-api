using System.Security.Claims;

namespace MiniUrl.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    public string GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    }
}