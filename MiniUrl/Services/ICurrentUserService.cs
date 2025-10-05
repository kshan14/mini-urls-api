namespace MiniUrl.Services;

public interface ICurrentUserService
{
    string GetUserId();
    string GetUserEmail();
}
