using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email address")
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(150).WithMessage("Email can be maximum 150 characters long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit");
        }
    }
}