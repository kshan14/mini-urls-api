using StackExchange.Redis;

namespace MiniUrl.Services.Background;

public class TinyUrlStatusChangeReceiver : BackgroundService
{
    private readonly ILogger<TinyUrlStatusChangeReceiver> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public TinyUrlStatusChangeReceiver(ILogger<TinyUrlStatusChangeReceiver> logger,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlCreatedChannel),
            (channel, msg) =>
            {
                _logger.LogInformation("Received TinyUrlCreated channel: {Channel}, message: {Message}", channel, msg);
            });
        await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlApprovedChannel),
            (channel, msg) =>
            {
                _logger.LogInformation("Received TinyUrlApproved channel: {Channel}, message: {Message}", channel, msg);
            });
        await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlRejectedChanel),
            (channel, msg) =>
            {
                _logger.LogInformation("Received TinyUrlRejected channel: {Channel}, message: {Message}", channel, msg);
            });
        _logger.LogInformation("Started TinyUrlStatusChangeReceiver");
        // let this function run indefinitely and only be cancelled by server
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();
        await subscriber.UnsubscribeAllAsync();
        _logger.LogInformation("TinyUrlStatusChangeReceiver stopped");
        await base.StopAsync(stoppingToken);
    }
}