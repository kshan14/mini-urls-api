using MiniUrl.Models.Requests.Login;
using MiniUrl.Models.Responses.Login;

namespace MiniUrl.Services;

public interface IAuthService
{
    Task<LoginResponse> Login(LoginRequest loginRequest);
}
