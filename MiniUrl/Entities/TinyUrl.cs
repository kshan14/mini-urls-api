namespace MiniUrl.Entities;

public class TinyUrl
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string ShortenedUrl { get; set; }
    public string Description { get; set; }
    public UrlStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid CreatorId { get; set; }
    public User Creator { get; set; }
    public Guid? ApproverId { get; set; }
    public User? Approver { get; set; }
}