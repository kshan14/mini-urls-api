using MiniUrl.Entities;

namespace MiniUrl.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
