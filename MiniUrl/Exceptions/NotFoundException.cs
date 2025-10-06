using MiniUrl.Models.Responses;

namespace MiniUrl.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public ErrorResponse ToErrorResponse(HttpContext httpContext)
    {
        var error = new ValidationError
        {
            Field = "",
            Message = message,
        };
        return new ErrorResponse
        {
            Title = "NOT_FOUND_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = StatusCodes.Status404NotFound,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = [error]
        };
    }
}