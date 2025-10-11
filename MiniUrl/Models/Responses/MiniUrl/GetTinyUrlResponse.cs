using MiniUrl.Models.Responses.User;

namespace MiniUrl.Models.Responses.MiniUrl;

public class GetTinyUrlResponse : CreateMiniUrlResponse
{
    public GetUserResponse CreatedBy { get; set; }
    public GetUserResponse? ApprovedBy { get; set; }
}