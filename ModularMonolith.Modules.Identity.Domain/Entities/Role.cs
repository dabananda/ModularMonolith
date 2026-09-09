using ModularMonolith.Shared.Entities;

namespace ModularMonolith.Modules.Identity.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        private readonly List<UserRole> _userRoles = [];
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private Role() { }

        public static Role Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));
            return new Role { Name = name, Description = description };
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));
            Name = name;
        }

        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            Name = name;
            Description = description;
        }

        public void MarkDelete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
        }
    }
}