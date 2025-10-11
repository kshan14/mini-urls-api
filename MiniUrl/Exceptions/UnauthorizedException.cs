using MiniUrl.Models.Responses;

namespace MiniUrl.Exceptions;

public class UnauthorizedException : Exception
{
    public ErrorResponse ToErrorResponse(HttpContext httpContext)
    {
        var error = new ValidationError
        {
            Field = "",
            Message = "Unauthorized Error"
        };
        return new ErrorResponse
        {
            Title = "UNAUTHORIZED_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = StatusCodes.Status401Unauthorized,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = [error]
        };
    }
}
