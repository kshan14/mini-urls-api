using MiniUrl.Entities;

namespace MiniUrl.Services;

public interface ICurrentUserService
{
    string GetUserId();
    string GetUserEmail();
    string GetUserRole();
    bool IsSameRole(Role userRole);
}
