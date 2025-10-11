using MiniUrl.Models.Requests.Common;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses.Common;
using MiniUrl.Models.Responses.MiniUrl;

namespace MiniUrl.Services;

public interface IMiniUrlGenerator
{
    Task<CreateMiniUrlResponse> GenerateUrl(CreateMiniUrlRequest req);
    Task ApproveUrl(Guid urlId);
    Task DenyUrl(Guid urlId);
}
