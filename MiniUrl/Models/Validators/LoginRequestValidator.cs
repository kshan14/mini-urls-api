using FluentValidation;
using MiniUrl.Models.Requests.Login;

namespace MiniUrl.Models.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("username is required")
            .MaximumLength(50).WithMessage("username must not exceed 50 characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("password is required")
            .MaximumLength(50).WithMessage("password must not exceed 50 characters");
    }
}
