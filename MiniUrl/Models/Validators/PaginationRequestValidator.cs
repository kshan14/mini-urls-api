using FluentValidation;
using MiniUrl.Models.Requests.Common;

namespace MiniUrl.Models.Validators;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    private const int MaxLimit = 100;

    public PaginationRequestValidator()
    {
        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0).WithMessage("offset must be positive number");
        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(0).WithMessage("limit must be positive number")
            .LessThanOrEqualTo(MaxLimit).WithMessage($"limit cannot be more than {MaxLimit}");
    }
}