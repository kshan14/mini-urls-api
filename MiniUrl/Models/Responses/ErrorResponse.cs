namespace MiniUrl.Models.Responses;

public class ErrorResponse
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Method { get; set; }
    public int StatusCode { get; set; }
    public string TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    public List<ValidationError> Errors { get; set; }
}
