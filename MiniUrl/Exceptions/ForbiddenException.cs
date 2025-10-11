using MiniUrl.Models.Responses;

namespace MiniUrl.Exceptions;

public class ForbiddenException : Exception
{
    public ErrorResponse ToErrorResponse(HttpContext httpContext)
    {
        var error = new ValidationError
        {
            Field = "",
            Message = "Forbidden Error"
        };
        return new ErrorResponse
        {
            Title = "FORBIDDEN_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = StatusCodes.Status403Forbidden,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = [error]
        };
    }
}
