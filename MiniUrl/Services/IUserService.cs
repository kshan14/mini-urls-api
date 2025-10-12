using MiniUrl.Models.Requests.User;
using MiniUrl.Models.Responses.User;

namespace MiniUrl.Services;

public interface IUserService
{
    Task<GetUserResponse> CreateUser(CreateUserRequest request);
}