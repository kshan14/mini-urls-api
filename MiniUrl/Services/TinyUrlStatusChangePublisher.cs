using System.Text.Json;
using MiniUrl.Entities;
using MiniUrl.Models.PubSub;
using StackExchange.Redis;

namespace MiniUrl.Services;

public class TinyUrlStatusChangePublisher : ITinyUrlStatusChangePublisher
{
    private readonly ILogger<TinyUrlStatusChangePublisher> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public TinyUrlStatusChangePublisher(ILogger<TinyUrlStatusChangePublisher> logger,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task PublishTinyUrlCreatedEvent(TinyUrl tinyUrl)
    {
        try
        {
            var receiverCount = await SerializeAndPublish(tinyUrl, Commons.Constants.TinyUrlCreatedChannel);
            _logger.LogInformation("Tiny Url Created Event published and receiver count: {ReceiverCount}",
                receiverCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing Tiny Url Created Event");
        }
    }

    public async Task PublishTinyUrlApprovedEvent(TinyUrl tinyUrl)
    {
        try
        {
            var receiverCount = await SerializeAndPublish(tinyUrl, Commons.Constants.TinyUrlApprovedChannel);
            _logger.LogInformation("Tiny Url Approved Event published and receiver count: {ReceiverCount}",
                receiverCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing Tiny Url Approved Event");
        }
    }

    public async Task PublishTinyUrlRejectedEvent(TinyUrl tinyUrl)
    {
        try
        {
            var receiverCount = await SerializeAndPublish(tinyUrl, Commons.Constants.TinyUrlRejectedChanel);
            _logger.LogInformation("Tiny Url Rejected Event published and receiver count: {ReceiverCount}",
                receiverCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing Tiny Url Rejected Event");
        }
    }

    private async Task<long> SerializeAndPublish(TinyUrl tinyUrl, string channel)
    {
        // 1. Transform to Pub/Sub Model
        // 2. Serialize JSON
        // 3. Publish to Given Channel
        var eventMsg = GetEvent(tinyUrl);
        var jsonStr = JsonSerializer.Serialize(eventMsg, Commons.Constants.JsonSerializerOptions);
        var subscriber = _connectionMultiplexer.GetSubscriber();
        return await subscriber.PublishAsync(RedisChannel.Literal(channel), jsonStr);
    }

    private TinyUrlStatusUpdate GetEvent(TinyUrl tinyUrl)
    {
        return new TinyUrlStatusUpdate
        {
            Id = tinyUrl.Id,
            Url = tinyUrl.Url,
            ShortenedUrl = tinyUrl.ShortenedUrl,
            Status = tinyUrl.Status.ToString(),
            CreatorId = tinyUrl.CreatorId,
            CreatedAt = tinyUrl.CreatedAt,
            UpdatedAt = tinyUrl.UpdatedAt,
            ExpiresAt = tinyUrl.ExpiresAt,
        };
    }
}
