using MiniUrl.Exceptions;

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
            _ => new InternalServerException().ToErrorResponse(httpContext)
        };
        var req = httpContext.Request;
        _logger.LogInformation(
            "Handling exception for path: {Path}, method: {Method}, traceId: {TraceId}, statusCode: {StatusCode}",
            req.Path, req.Method, httpContext.TraceIdentifier, errorResponse.StatusCode);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = errorResponse.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }
}
