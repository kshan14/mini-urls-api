using MiniUrl.Models.Responses;

namespace MiniUrl.Exceptions;

public class InternalServerException : Exception
{
    public ErrorResponse ToErrorResponse(HttpContext httpContext)
    {
        var error = new ValidationError
        {
            Field = "",
            Message = "Internal Server Error"
        };
        return new ErrorResponse
        {
            Title = "INTERNAL_SERVER_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = StatusCodes.Status500InternalServerError,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = [error]
        };
    }
}