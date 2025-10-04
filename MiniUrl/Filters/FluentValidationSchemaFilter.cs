using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using FluentValidation;
using FluentValidation.Validators;

namespace MiniUrl.Filters;

public class FluentValidationSchemaFilter : ISchemaFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationSchemaFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
            return;

        // Create a scope to resolve scoped validators
        using var scope = _serviceProvider.CreateScope();
        var validatorType = typeof(IValidator<>).MakeGenericType(context.Type);
        var validator = scope.ServiceProvider.GetService(validatorType) as IValidator;

        if (validator == null)
            return;

        var descriptor = validator.CreateDescriptor();

        foreach (var property in schema.Properties)
        {
            var propertyName = char.ToUpper(property.Key[0]) + property.Key.Substring(1);
            var validators = descriptor.GetValidatorsForMember(propertyName);

            foreach (var validatorComponent in validators)
            {
                ApplyValidatorRules(schema, property.Key, property.Value, validatorComponent.Validator);
            }
        }
    }

    private void ApplyValidatorRules(OpenApiSchema schema, string propertyKey, OpenApiSchema propertySchema, IPropertyValidator validator)
    {
        // NotEmpty/NotNull - make field required
        if (validator is INotNullValidator or INotEmptyValidator)
        {
            if (!schema.Required.Contains(propertyKey))
                schema.Required.Add(propertyKey);
            propertySchema.Nullable = false;
        }

        // Length validators
        if (validator is ILengthValidator lengthValidator)
        {
            if (lengthValidator.Max > 0)
                propertySchema.MaxLength = lengthValidator.Max;
            if (lengthValidator.Min > 0)
                propertySchema.MinLength = lengthValidator.Min;
        }

        // Comparison validators (GreaterThan, LessThan, etc.)
        if (validator is IComparisonValidator comparisonValidator)
        {
            var valueToCompare = comparisonValidator.ValueToCompare;
            var validatorName = validator.Name;
            
            if (validatorName.Contains("GreaterThan"))
            {
                if (decimal.TryParse(valueToCompare?.ToString(), out var min))
                {
                    propertySchema.Minimum = min;
                    if (!validatorName.Contains("OrEqual"))
                        propertySchema.ExclusiveMinimum = true;
                }
            }
            else if (validatorName.Contains("LessThan"))
            {
                if (decimal.TryParse(valueToCompare?.ToString(), out var max))
                {
                    propertySchema.Maximum = max;
                    if (!validatorName.Contains("OrEqual"))
                        propertySchema.ExclusiveMaximum = true;
                }
            }
        }

        // Email validator
        if (validator is IEmailValidator)
        {
            propertySchema.Format = "email";
        }

        // Regular expression validator
        if (validator is IRegularExpressionValidator regexValidator)
        {
            propertySchema.Pattern = regexValidator.Expression;
        }
    }
}