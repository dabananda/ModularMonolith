namespace ModularMonolith.Modules.Identity.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;
        public Role Role { get; private set; } = null!;

        private UserRole() { }

        internal static UserRole Create(ApplicationUser user, Role role) =>
            new() { UserId = user.Id, User = user, RoleId = role.Id, Role = role };
    }
}