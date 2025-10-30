using Microsoft.EntityFrameworkCore;
using MiniUrl.Configs;
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
    private readonly UrlConfig _urlConfig;
    private readonly AppDbContext _appDbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapperService _mapperService;
    private readonly IUrlCacheService _urlCacheService;

    public MiniUrlViewService(
        ILogger<MiniUrlViewService> logger,
        UrlConfig urlConfig,
        AppDbContext appDbContext,
        ICurrentUserService currentUserService,
        IMapperService mapperService,
        IUrlCacheService urlCacheService
    )
    {
        _logger = logger;
        _urlConfig = urlConfig;
        _currentUserService = currentUserService;
        _appDbContext = appDbContext;
        _mapperService = mapperService;
        _urlCacheService = urlCacheService;
    }

    public async Task<PaginationResponse<GetTinyUrlResponse>> GetUrls(PaginationRequest req, UrlStatus? status)
    {
        try
        {
            // 1. Get the data list
            // 2. Get total count
            var list = await GetPaginatedTinyUrls(req, status);
            var count = await GetTinyUrlsTotalCountQueryable(status)
                .LongCountAsync();

            var dataList = list.Select(t =>
                {
                    var res = _mapperService.GetTinyUrlResponse(t);
                    res.ShortenedUrl = Path.Combine(_urlConfig.BasePath, t.ShortenedUrl);
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

    public async Task<string> GetUrl(string shortenedPath)
    {
        try
        {
            // 1. First look by cache
            var redirectUrl = await _urlCacheService.GetRedirectedUrl(shortenedPath);
            if (redirectUrl != null)
            {
                return redirectUrl;
            }

            // 2. Not found in cache, look in DB. Only look for Approved Url
            var tinyUrl = await _appDbContext.TinyUrls
                .Where(x => x.Status.Equals(UrlStatus.Approved))
                .Where(x => x.ShortenedUrl.Equals(shortenedPath))
                .FirstOrDefaultAsync();
            if (tinyUrl == null)
                throw new NotFoundException($"{shortenedPath} not found");

            // 3. Save in Cache, this should be in fire-and-forget pattern
            _ = Task.Run(async () => { await SaveInCache(tinyUrl); });

            // 4. Return Original Url
            return tinyUrl.Url;
        }
        catch (Exception ex) when (ex is NotFoundException)
        {
            _logger.LogError(ex, "Cannot find {ShortenedPath}", shortenedPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {ShortenedPath}", shortenedPath);
            throw new InternalServerException();
        }
    }

    private async Task SaveInCache(TinyUrl tinyUrl)
    {
        // failing to store data in cache should not throw an error
        try
        {
            await _urlCacheService.SaveRedirectedUrl(tinyUrl.ShortenedUrl, tinyUrl.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving in cache for {ShortenedPath}", tinyUrl.ShortenedUrl);
        }
    }

    private async Task<List<TinyUrl>> GetPaginatedTinyUrls(PaginationRequest req, UrlStatus? status)
    {
        // 1. Get the paginated record id with filters. As Include Join interrupts Take Query.
        var recordIdsQuery = _appDbContext.TinyUrls.AsQueryable();
        // filter based on role
        if (_currentUserService.IsSameRole(Role.User))
        {
            recordIdsQuery = recordIdsQuery.Where(t => t.CreatorId.Equals(_currentUserService.GetUserId()));
        }
        // filter based on status if applicable
        if (status != null)
        {
            recordIdsQuery = recordIdsQuery.Where(t => t.Status.Equals(status));
        }

        var recordIds = await recordIdsQuery
            .OrderByDescending(r => r.UpdatedAt)
            .Skip(req.Offset)
            .Take(req.Limit)
            .Select(r => r.Id).ToListAsync();
        
        // once get the paginated record id, take the records by join
        return await _appDbContext.TinyUrls
            .Where(r => recordIds.Contains(r.Id))
            .Include(r => r.Approver)
            .Include(r => r.Creator)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }

    private IQueryable<TinyUrl> GetTinyUrlsTotalCountQueryable(UrlStatus? status)
    {
        var countQuery = _appDbContext.TinyUrls.AsQueryable();

        if (_currentUserService.IsSameRole(Role.User))
        {
            countQuery = countQuery.Where(t => t.CreatorId.Equals(_currentUserService.GetUserId()));
        }

        if (status != null)
        {
            countQuery = countQuery.Where(t => t.Status.Equals(status));
        }

        return countQuery;
    }
}