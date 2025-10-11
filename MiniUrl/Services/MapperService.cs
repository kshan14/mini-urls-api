using MiniUrl.Entities;
using MiniUrl.Models.Responses.MiniUrl;
using MiniUrl.Models.Responses.User;

namespace MiniUrl.Services;

public class MapperService : IMapperService
{
    public GetTinyUrlResponse GetTinyUrlResponse(TinyUrl tinyUrl)
    {
        return new GetTinyUrlResponse
        {
            Id = tinyUrl.Id,
            Url = tinyUrl.Url,
            ShortenedUrl = tinyUrl.ShortenedUrl,
            Description = tinyUrl.Description,
            Status = nameof(tinyUrl.Status),
            CreatedAt = tinyUrl.CreatedAt,
            UpdatedAt = tinyUrl.UpdatedAt,
            ExpiresAt = tinyUrl.ExpiresAt
        };
    }

    public GetUserResponse? GetUserResponse(User? user)
    {
        if (user == null)
            return null;
        return new GetUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = nameof(user.Role)
        };
    }
}
