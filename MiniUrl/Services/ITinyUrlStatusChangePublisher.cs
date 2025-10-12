using MiniUrl.Entities;

namespace MiniUrl.Services;

public interface ITinyUrlStatusChangePublisher
{
    Task PublishTinyUrlCreatedEvent(TinyUrl tinyUrl);
    Task PublishTinyUrlApprovedEvent(TinyUrl tinyUrl);
    Task PublishTinyUrlRejectedEvent(TinyUrl tinyUrl);
}