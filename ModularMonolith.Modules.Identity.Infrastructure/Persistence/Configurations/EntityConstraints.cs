namespace ModularMonolith.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public static class EntityConstraints
    {
        public const int EmailMaxLength = 256;
        public const int DomainMaxLength = 256;
        public const int UserNameMaxLength = 256;
        public const int RoleNameMaxLength = 100;
        public const int PasswordHashMaxLength = 500;
        public const int TokenMaxLength = 500;
    }
}
