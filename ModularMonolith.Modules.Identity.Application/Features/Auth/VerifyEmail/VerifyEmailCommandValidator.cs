using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token is required");
        }
    }
}