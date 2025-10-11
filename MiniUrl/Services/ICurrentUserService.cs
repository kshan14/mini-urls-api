using MiniUrl.Entities;

namespace MiniUrl.Services;

public interface ICurrentUserService
{
    Guid GetUserId();
    string GetUserEmail();
    Role GetUserRole();
    bool IsSameRole(Role userRole);
}
