using MiniUrl.Entities;
using MiniUrl.Models.Requests.Common;
using MiniUrl.Models.Responses.Common;
using MiniUrl.Models.Responses.MiniUrl;

namespace MiniUrl.Services;

public interface IMiniUrlViewService
{
    Task<PaginationResponse<GetTinyUrlResponse>> GetUrls(PaginationRequest req, UrlStatus? filter);
    Task<string> GetUrl(string shortenedPath);
}