using FluentValidation;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Update
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Role id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(50).WithMessage("Role name should be less than 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description should be less than 200 characters");
        }
    }
}