using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Remove
{
    public class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
    {
        public RemoveRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("Role ID is required.");
        }
    }
}
