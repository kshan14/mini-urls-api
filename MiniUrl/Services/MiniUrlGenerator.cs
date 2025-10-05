using Microsoft.EntityFrameworkCore;
using MiniUrl.Data;
using MiniUrl.Entities;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses.MiniUrl;
using Npgsql;

namespace MiniUrl.Services;

public class MiniUrlGenerator : IMiniUrlGenerator
{
    private readonly ILogger<MiniUrlGenerator> _logger;
    private readonly IBase62Encoder _base62Encoder;
    private readonly IUrlCounter _urlCounter;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _appDbContext;
    private readonly int ConflictRetryTimes = 10;

    public MiniUrlGenerator(
        ILogger<MiniUrlGenerator> logger,
        IBase62Encoder base62Encoder,
        IUrlCounter urlCounter,
        ICurrentUserService currentUserService,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _base62Encoder = base62Encoder;
        _urlCounter = urlCounter;
        _currentUserService = currentUserService;
        _appDbContext = appDbContext;
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
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _logger.LogWarning(ex, "Unique constraint violation");
            }
        }
        _logger.LogError("Failed to create new url with retry times {ConflictRetryTimes} exceeded", ConflictRetryTimes);
        throw new Exception("Failed to create url");
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
                CreatorId = Guid.Parse(_currentUserService.GetUserId()),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMonths(5),
            };
            _appDbContext.TinyUrls.Add(miniUrlRecord);
            await _appDbContext.SaveChangesAsync();
            await txn.CommitAsync();
            return new CreateMiniUrlResponse
            {
                Id = miniUrlRecord.Id,
                Url = miniUrlRecord.Url,
                ShortenedUrl = miniUrlRecord.ShortenedUrl,
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

    private bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pgEx)
        {
            return pgEx.SqlState == PostgresErrorCodes.UniqueViolation;
        }

        return false;
    }
}
