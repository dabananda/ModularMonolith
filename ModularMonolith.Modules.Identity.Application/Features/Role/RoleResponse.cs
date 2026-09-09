namespace ModularMonolith.Modules.Identity.Application.Features.Role
{
    public record RoleResponse(
        Guid Id,
        string Name,
        string? Description);
}