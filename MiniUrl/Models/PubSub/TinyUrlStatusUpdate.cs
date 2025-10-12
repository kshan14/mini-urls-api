using MiniUrl.Entities;

namespace MiniUrl.Models.PubSub;

public class TinyUrlStatusUpdate
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string ShortenedUrl { get; set; }
    public UrlStatus Status { get; set; }
    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
