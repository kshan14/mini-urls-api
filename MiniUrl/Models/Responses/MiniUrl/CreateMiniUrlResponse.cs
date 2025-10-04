namespace MiniUrl.Models.Responses.MiniUrl;

public class CreateMiniUrlResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string ShortenedUrl { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}