namespace MiniUrl.Models.Requests.Common;

public class PaginationRequest
{
    private const int DefaultOffset = 0;
    private const int DefaultLimit = 10;

    public int Offset { get; set; } = DefaultOffset;

    public int Limit { get; set; } = DefaultLimit;
}