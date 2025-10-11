using Microsoft.EntityFrameworkCore;
using MiniUrl.Data;
using MiniUrl.Entities;
using MiniUrl.Exceptions;
using MiniUrl.Models.Requests.Common;
using MiniUrl.Models.Responses.Common;
using MiniUrl.Models.Responses.MiniUrl;

namespace MiniUrl.Services;

public class MiniUrlViewService : IMiniUrlViewService
{
    private readonly ILogger<MiniUrlViewService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapperService _mapperService;

    public MiniUrlViewService(
        ILogger<MiniUrlViewService> logger,
        ICurrentUserService currentUserService,
        IMapperService mapperService,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _appDbContext = appDbContext;
        _mapperService = mapperService;
    }

    public async Task<PaginationResponse<GetTinyUrlResponse>> GetUrls(PaginationRequest req)
    {
        try
        {
            // 1. Get the data list
            // 2. Get total count
            var list = await GetTinyUrlsListQueryable(req)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
            var count = await GetTinyUrlsTotalCountQueryable()
                .LongCountAsync();

            var dataList = list.Select(t =>
                {
                    var res = _mapperService.GetTinyUrlResponse(t);
                    res.CreatedBy = _mapperService.GetUserResponse(t.Creator)!;
                    res.ApprovedBy = _mapperService.GetUserResponse(t.Approver);
                    return res;
                })
                .ToList();

            return new PaginationResponse<GetTinyUrlResponse>
            {
                Data = dataList,
                Length = dataList.Count,
                TotalCount = count,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting urls");
            throw new InternalServerException();
        }
    }

    private IQueryable<TinyUrl> GetTinyUrlsListQueryable(PaginationRequest req)
    {
        var listQuery = _appDbContext.TinyUrls.AsQueryable()
            .Skip(req.Offset)
            .Take(req.Limit);

        if (_currentUserService.IsSameRole(Role.User))
        {
            listQuery = listQuery.Where(t => t.CreatorId.Equals(_currentUserService.GetUserId()));
        }

        return listQuery.Include(t => t.Creator)
            .Include(t => t.Approver);
    }

    private IQueryable<TinyUrl> GetTinyUrlsTotalCountQueryable()
    {
        var countQuery = _appDbContext.TinyUrls.AsQueryable();

        if (_currentUserService.IsSameRole(Role.User))
        {
            countQuery = countQuery.Where(t => t.CreatorId.Equals(_currentUserService.GetUserId()));
        }

        return countQuery;
    }
}