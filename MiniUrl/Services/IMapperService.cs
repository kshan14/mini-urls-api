using MiniUrl.Entities;
using MiniUrl.Models.Responses.MiniUrl;
using MiniUrl.Models.Responses.User;

namespace MiniUrl.Services;

public interface IMapperService
{
    GetTinyUrlResponse GetTinyUrlResponse(TinyUrl tinyUrl);
    GetUserResponse? GetUserResponse(User? user);
}
