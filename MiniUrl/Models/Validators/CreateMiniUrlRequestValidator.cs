using FluentValidation;
using MiniUrl.Models.Requests.MiniUrl;

namespace MiniUrl.Models.Validators;

public class CreateMiniUrlRequestValidator : AbstractValidator<CreateMiniUrlRequest>
{
    private static readonly ICollection<string> AllowedUriSchemes = new List<string>
    {
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeFtp,
    };
    
    public CreateMiniUrlRequestValidator()
    {
        RuleFor(u => u.Url)
            .NotEmpty().WithMessage("Url is required")
            .MaximumLength(2000).WithMessage("Url cannot exceed 2000 characters")
            .Must(IsValidUrl).WithMessage("Url must be in a valid format (http://, https://, ftp://)");
    }

    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // just basic url
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && AllowedUriSchemes.Contains(uriResult.Scheme);
    }
}