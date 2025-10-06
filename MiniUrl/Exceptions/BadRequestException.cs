using MiniUrl.Models.Responses;

namespace MiniUrl.Exceptions;

public class BadRequestException(string message) : Exception(message)
{
    public ErrorResponse ToErrorResponse(HttpContext httpContext)
    {
        var error = new ValidationError
        {
            Field = "",
            Message = message
        };
        return new ErrorResponse
        {
            Title = "BAD_REQUEST_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = StatusCodes.Status400BadRequest,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = [error]
        };
    }
}
