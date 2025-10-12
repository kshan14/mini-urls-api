using FluentValidation;
using MiniUrl.Models.Requests.User;

namespace MiniUrl.Models.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("username is required")
            .MaximumLength(50).WithMessage("username must not exceed 50 characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("password is required")
            .MaximumLength(50).WithMessage("password must not exceed 50 characters");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("email is required")
            .MaximumLength(50).WithMessage("email must not exceed 50 characters")
            .EmailAddress().WithMessage("email is not a valid email address");
    }
}
