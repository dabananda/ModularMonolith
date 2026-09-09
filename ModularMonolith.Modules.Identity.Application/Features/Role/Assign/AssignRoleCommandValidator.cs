using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Assign
{
    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
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
