using MiniUrl.Configs;
using StackExchange.Redis;

namespace MiniUrl.Services;

public class UrlCacheService : IUrlCacheService
{
    private const string CacheKey = "RedirectUrl";
    private readonly ILogger<UrlCacheService> _logger;
    private readonly UrlConfig _urlConfig;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public UrlCacheService(ILogger<UrlCacheService> logger, UrlConfig urlConfig, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _urlConfig = urlConfig;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<string?> GetRedirectedUrl(string shortenedPath)
    {
        // key -> RedirectUrl:abc, value -> http://abc.com/my-long-url/my-long-sub-url
        var key = BuildFullCacheKey(shortenedPath);
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.HasValue)
        {
            return value;
        }

        return null;
    }

    public async Task RemoveRedirectedUrl(string shortenedPath)
    {
        var key = BuildFullCacheKey(shortenedPath);
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task SaveRedirectedUrl(string shortenedPath, string redirectedUrl)
    {
        var key = BuildFullCacheKey(shortenedPath);
        var db = _connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, redirectedUrl, TimeSpan.FromMinutes(_urlConfig.CacheExpiryMinutes));
    }

    private string BuildFullCacheKey(string shortenedPath)
    {
        return $"{CacheKey}:{shortenedPath}";
    }
}
