using System.Text.Json;
using MiniUrl.Models.PubSub;
using MiniUrl.Websockets;
using StackExchange.Redis;

namespace MiniUrl.Services.Background;

public class TinyUrlStatusChangeReceiver : BackgroundService
{
    private readonly ILogger<TinyUrlStatusChangeReceiver> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IWebSocketManager _webSocketManager;

    public TinyUrlStatusChangeReceiver(ILogger<TinyUrlStatusChangeReceiver> logger,
        IConnectionMultiplexer connectionMultiplexer, IWebSocketManager webSocketManager)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _webSocketManager = webSocketManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var subscriber = _connectionMultiplexer.GetSubscriber();
            await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlCreatedChannel),
                OnTinyUrlCreated);
            await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlApprovedChannel),
                OnTinyUrlApproved);
            await subscriber.SubscribeAsync(RedisChannel.Literal(Commons.Constants.TinyUrlRejectedChanel),
                OnTinyUrlRejected);
            _logger.LogInformation("Successfully subscribe to redis channels");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to redis channels");
        }

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

    // OnTinyUrlCreated forward tiny url created record event to all admins for decision
    private void OnTinyUrlCreated(RedisChannel channel, RedisValue msg)
    {
        _logger.LogInformation("Received TinyUrlCreated channel: {Channel}", channel);
        if (!msg.HasValue)
        {
            _logger.LogInformation("TinyUrlCreated channel: {Channel} receives empty message", channel);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                await _webSocketManager.SendToAllAdminsAsync(msg!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TinyUrlCreated channel");
            }
        });
    }

    // OnTinyUrlApproved forward tiny url record approved event to creator for outcome
    private void OnTinyUrlApproved(RedisChannel channel, RedisValue msg)
    {
        _logger.LogInformation("Received TinyUrlApproved channel: {Channel}", channel);
        if (!msg.HasValue)
        {
            _logger.LogInformation("TinyUrlApproved channel: {Channel} receives empty message", channel);
            return;
        }

        ParseAndSendToUser(msg, channel.ToString());
    }

    // OnTinyUrlRejected forward tiny url record rejected event to creator for outcome
    private void OnTinyUrlRejected(RedisChannel channel, RedisValue msg)
    {
        _logger.LogInformation("Received TinyUrlRejected channel: {Channel}", channel);
        if (!msg.HasValue)
        {
            _logger.LogInformation("TinyUrlRejected channel: {Channel} receives empty message", channel);
            return;
        }

        ParseAndSendToUser(msg, channel.ToString());
    }

    private void ParseAndSendToUser(RedisValue msg, string channel)
    {
        var eventUpdate = ParseMsg(msg);
        if (eventUpdate == null)
        {
            _logger.LogInformation("Parsing Message from channel: {Channel} got null object", channel);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                await _webSocketManager.SendToUserAsync(eventUpdate.CreatorId, msg!);
                _logger.LogInformation("Sent message from channel: {Channel} to user", channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from channel: {Channel} to user clients", channel);
            }
        });
    }

    private TinyUrlStatusUpdate? ParseMsg(RedisValue msg)
    {
        try
        {
            return JsonSerializer.Deserialize<TinyUrlStatusUpdate>(msg.ToString(),
                Commons.Constants.JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing to TinyUrlStatusUpdate");
            return null;
        }
    }
}