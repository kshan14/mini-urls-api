namespace MiniUrl.Models.Responses.Common;

public class PaginationResponse<T>
{
    public ICollection<T> Data { get; set; } = new List<T>();
    public int Length { get; set; }
    public long TotalCount { get; set; }
}
