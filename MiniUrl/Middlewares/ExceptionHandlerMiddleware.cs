using MiniUrl.Exceptions;
using MiniUrl.Models.Responses;

namespace MiniUrl.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        var errorResponse = ex switch
        {
            BadRequestException badRequestException => badRequestException.ToErrorResponse(httpContext),
            NotFoundException notFoundException => notFoundException.ToErrorResponse(httpContext),
            InternalServerException internalServerException => internalServerException.ToErrorResponse(httpContext),
            _ => HandleUnrecognisedException(ex, httpContext)
        };
        var req = httpContext.Request;
        LogErrorResponse(errorResponse, ex, httpContext, "Handling Recognised Exception");

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = errorResponse.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }

    private ErrorResponse HandleUnrecognisedException(Exception ex, HttpContext httpContext)
    {
        var errorResponse = new InternalServerException().ToErrorResponse(httpContext);
        LogErrorResponse(errorResponse, ex, httpContext, "Handling UnrecognisedException");
        return new InternalServerException().ToErrorResponse(httpContext);
    }

    private void LogErrorResponse(ErrorResponse errorResponse, Exception ex, HttpContext httpContext, string msgPrefix)
    {
        var req = httpContext.Request;
        _logger.LogError(ex,
            "{MessagePrefix} for path: {Path}, method: {Method}, traceId: {TraceId}, statusCode: {StatusCode}",
            msgPrefix,
            req.Path, req.Method, httpContext.TraceIdentifier, errorResponse.StatusCode);
    }
}
