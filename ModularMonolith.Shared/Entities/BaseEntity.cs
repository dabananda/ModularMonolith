namespace ModularMonolith.Shared.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected internal set; }

        public bool IsDeleted { get; protected internal set; }

        public DateTime CreatedAt { get; protected internal set; }
        public DateTime? UpdatedAt { get; protected internal set; }
        public DateTime? DeletedAt { get; protected internal set; }

        public Guid? CreatedBy { get; protected internal set; }
        public Guid? UpdatedBy { get; protected internal set; }
        public Guid? DeletedBy { get; protected internal set; }
    }
}
