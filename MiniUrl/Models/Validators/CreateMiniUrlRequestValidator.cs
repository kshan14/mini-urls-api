using FluentValidation;
using MiniUrl.Models.Requests.MiniUrl;

namespace MiniUrl.Models.Validators;

public class CreateMiniUrlRequestValidator : AbstractValidator<CreateMiniUrlRequest>
{
    private static readonly List<string> AllowedUriSchemes =
    [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeFtp,
    ];

    public CreateMiniUrlRequestValidator()
    {
        RuleFor(u => u.Url)
            .NotEmpty().WithMessage("url is required")
            .MaximumLength(2000).WithMessage("url cannot exceed 2000 characters")
            .Must(IsValidUrl).WithMessage("url must be in a valid format (http://, https://, ftp://)");
        RuleFor(u => u.Description)
            .MaximumLength(2000).WithMessage("description cannot exceed 2000 characters");
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
