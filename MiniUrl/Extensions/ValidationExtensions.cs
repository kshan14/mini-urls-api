using FluentValidation.Results;

namespace MiniUrl.Extensions;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToDictionary(this ValidationResult result)
    {
        return result.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
    }
}
