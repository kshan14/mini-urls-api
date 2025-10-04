using System.Text.Json;
using FluentValidation.Results;
using MiniUrl.Models.Responses;

namespace MiniUrl.Extensions;

public static class ValidationExtensions
{
    public static ErrorResponse ToErrorResponse(this ValidationResult result, HttpContext httpContext)
    {
        var namingPolicy = JsonNamingPolicy.CamelCase;
        var errors = result.Errors
            .Select(x => new ValidationError
            {
                Field = namingPolicy.ConvertName(x.PropertyName),
                Message = x.ErrorMessage
            })
            .ToList();
        return new ErrorResponse
        {
            Title = "VALIDATION_ERROR",
            Url = httpContext.Request.Path,
            Method = httpContext.Request.Method,
            StatusCode = (int)StatusCodes.Status400BadRequest,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.Now,
            Errors = errors
        };
    }
}
