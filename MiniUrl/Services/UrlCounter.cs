using StackExchange.Redis;
using MiniUrl.Extensions;

namespace MiniUrl.Services;

public class UrlCounter : IUrlCounter
{
    private const string CounterKey = "UrlCounter";
    private readonly ILogger<UrlCounter> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public UrlCounter(ILogger<UrlCounter> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<long> GetIncrementalCounter()
    {
        _logger.LogInformation("Getting incremental counter");
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringIncrementAsync(CounterKey).ConfigureAwait(false);
        _logger.LogInformation("IncrementalCounter returned {Value}", value);
        return value;
    }
}
