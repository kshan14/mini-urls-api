using Microsoft.EntityFrameworkCore;
using MiniUrl.Configs;
using MiniUrl.Data;
using MiniUrl.Entities;
using MiniUrl.Exceptions;
using MiniUrl.Models.Requests.Common;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses.Common;
using MiniUrl.Models.Responses.MiniUrl;
using Npgsql;

namespace MiniUrl.Services;

public class MiniUrlGenerator : IMiniUrlGenerator
{
    private readonly ILogger<MiniUrlGenerator> _logger;
    private readonly IBase62Encoder _base62Encoder;
    private readonly IUrlCounter _urlCounter;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUrlCacheService _urlCacheService;
    private readonly ITinyUrlStatusChangePublisher _tinyUrlStatusChangePublisher;
    private readonly AppDbContext _appDbContext;
    private readonly UrlConfig _urlConfig;
    private readonly int ConflictRetryTimes = 1000;
    private readonly int LockTimeoutSeconds = 30;

    public MiniUrlGenerator(
        ILogger<MiniUrlGenerator> logger,
        IBase62Encoder base62Encoder,
        IUrlCounter urlCounter,
        ICurrentUserService currentUserService,
        ITinyUrlStatusChangePublisher tinyUrlStatusChangePublisher,
        IUrlCacheService urlCacheService,
        AppDbContext appDbContext,
        UrlConfig urlConfig)
    {
        _logger = logger;
        _base62Encoder = base62Encoder;
        _urlCounter = urlCounter;
        _currentUserService = currentUserService;
        _urlCacheService = urlCacheService;
        _tinyUrlStatusChangePublisher = tinyUrlStatusChangePublisher;
        _appDbContext = appDbContext;
        _urlConfig = urlConfig;
    }

    public async Task<CreateMiniUrlResponse> GenerateUrl(CreateMiniUrlRequest req)
    {
        // try to create url for defined no of times if generated url has conflict
        for (var i = 0; i < ConflictRetryTimes; i++)
        {
            try
            {
                return await GetCounterAndGenerateUrl(req);
            }
            catch (DbUpdateException ex) when (Commons.Utilities.IsUniqueConstraintViolation(ex))
            {
                _logger.LogWarning(ex, "Unique constraint violation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating url");
                throw new InternalServerException();
            }
        }

        _logger.LogError("Failed to create new url with retry times {ConflictRetryTimes} exceeded", ConflictRetryTimes);
        throw new InternalServerException();
    }

    private async Task<CreateMiniUrlResponse> GetCounterAndGenerateUrl(CreateMiniUrlRequest req)
    {
        await using var txn = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Get counter
            var counter = await _urlCounter.GetIncrementalCounter();
            // 2. Generate Tiny Url on top of counter
            var miniUrl = _base62Encoder.Encode(counter);
            // 3. Save in DB
            var miniUrlRecord = new TinyUrl
            {
                Url = req.Url,
                ShortenedUrl = miniUrl,
                Description = req.Description,
                Status = UrlStatus.Pending,
                CreatorId = _currentUserService.GetUserId(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMonths(5),
            };
            _appDbContext.TinyUrls.Add(miniUrlRecord);
            await _appDbContext.SaveChangesAsync();
            await txn.CommitAsync();
            // 4. Publish to notify about new record creation
            _ = Task.Run(async () =>
            {
                await _tinyUrlStatusChangePublisher.PublishTinyUrlCreatedEvent(miniUrlRecord);
            });
            return new CreateMiniUrlResponse
            {
                Id = miniUrlRecord.Id,
                Url = miniUrlRecord.Url,
                ShortenedUrl = Path.Combine(_urlConfig.BasePath,  miniUrlRecord.ShortenedUrl),
                Description = miniUrlRecord.Description,
                Status = miniUrlRecord.Status.ToString(),
                CreatedAt = miniUrlRecord.CreatedAt,
                UpdatedAt = miniUrlRecord.UpdatedAt,
                ExpiresAt = miniUrlRecord.ExpiresAt,
            };
        }
        catch
        {
            await txn.RollbackAsync();
            throw;
        }
    }

    public async Task ApproveUrl(Guid urlId)
    {
        await using var txn = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Get Existing Record
            var entity = await GetTinyUrlWithLock(urlId);
            if (entity == null)
            {
                _logger.LogInformation("No TinyUrl with id {Id} found", urlId);
                throw new NotFoundException($"Url {urlId} not found!");
            }

            // 2. Validate status must not be already approved
            if (entity.Status is UrlStatus.Approved)
            {
                _logger.LogInformation("Url with id {UrlId}, tiny url {TinyUrl} already approved", urlId,
                    entity.ShortenedUrl);
                throw new BadRequestException($"Url already {entity.Status}");
            }

            // 3. Approve and Update in DB
            entity.Status = UrlStatus.Approved;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ApproverId = _currentUserService.GetUserId();
            await _appDbContext.SaveChangesAsync();
            await txn.CommitAsync();
            // 4. Publish to notify about record approval
            _ = Task.Run(async () => { await _tinyUrlStatusChangePublisher.PublishTinyUrlApprovedEvent(entity); });
            _logger.LogInformation("Url with id {UrlId} has been approved", urlId);
        }
        catch (Exception ex) when (ex is NotFoundException or BadRequestException)
        {
            await txn.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Failed to approve url with id {UrlId}", urlId);
            throw new InternalServerException();
        }
    }

    public async Task DenyUrl(Guid urlId)
    {
        await using var txn = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Get TinyUrl by id
            var entity = await GetTinyUrlWithLock(urlId);
            if (entity == null)
            {
                _logger.LogInformation("No TinyUrl with id {Id found}", urlId);
                throw new NotFoundException($"Url {urlId} not found!");
            }

            if (entity.Status is UrlStatus.Rejected)
            {
                _logger.LogInformation("Url with id {UrlId}, tiny url {TinyUrl} already rejected", urlId,
                    entity.ShortenedUrl);
                throw new BadRequestException($"Url already {entity.Status}");
            }

            // 2. Update attributes and save in DB
            entity.Status = UrlStatus.Rejected;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ApproverId = _currentUserService.GetUserId();
            await _appDbContext.SaveChangesAsync();
            await txn.CommitAsync();
            _logger.LogInformation("Url with id {UrlId} has been rejected", urlId);
            // 3. Clear from Cache
            _ = Task.Run(async () => { await RemoveTinyUrlFromCache(entity.ShortenedUrl); });
            // 4. Publish to notify about record rejection
            _ = Task.Run(async () => { await _tinyUrlStatusChangePublisher.PublishTinyUrlRejectedEvent(entity); });
        }
        catch (Exception ex) when (ex is NotFoundException or BadRequestException)
        {
            await txn.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Failed to reject url with id {UrlId}", urlId);
            throw new InternalServerException();
        }
    }

    // Note. Admin can delete any url. User can only delete his/her own url
    public async Task DeleteUrl(Guid urlId)
    {
        await using var txn = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Get Tiny Url by id
            var entity = await GetTinyUrlWithLock(urlId);
            if (entity == null)
            {
                _logger.LogInformation("No TinyUrl with id {Id found}", urlId);
                throw new NotFoundException($"Url {urlId} not found!");
            }

            // 2. Admin can delete any record. User can only delete his/her own url
            if (_currentUserService.IsSameRole(Role.User) && !entity.CreatorId.Equals(_currentUserService.GetUserId()))
            {
                _logger.LogInformation("User cannot delete the url which he/she doesn't own");
                throw new ForbiddenException();
            }

            // 3. Remove from DB
            _appDbContext.TinyUrls.Remove(entity);
            await _appDbContext.SaveChangesAsync();
            await txn.CommitAsync();
            _logger.LogInformation("Url with id {UrlId} has been deleted", urlId);
            // 4. Clear from cache
            _ = Task.Run(async () => { await RemoveTinyUrlFromCache(entity.ShortenedUrl); });
        }
        catch (Exception ex) when (ex is NotFoundException or ForbiddenException)
        {
            await txn.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
        }
    }

    private async Task<TinyUrl?> GetTinyUrlWithLock(Guid urlId)
    {
        await _appDbContext.Database.ExecuteSqlRawAsync($"SET lock_timeout = {LockTimeoutSeconds * 1000}");
        return await _appDbContext.TinyUrls.FromSqlRaw("""
                                                        SELECT * FROM "TinyUrls"
                                                        WHERE "Id" = {0} FOR UPDATE
                                                       """, urlId).FirstOrDefaultAsync();
    }

    private async Task RemoveTinyUrlFromCache(string shortenedUrl)
    {
        try
        {
            await _urlCacheService.RemoveRedirectedUrl(shortenedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing TinyUrl {ShortenedUrl} from cache", shortenedUrl);
        }
    }
}
