using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Create
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description should be less than 200 characters");
        }
    }
}
