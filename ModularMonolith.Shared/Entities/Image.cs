namespace ModularMonolith.Shared.Entities
{
    public class Image : BaseEntity
    {
        public Guid? EntityId { get; private set; }
        public string PublicId { get; private set; }
        public string Url { get; private set; }
        public string SecureUrl { get; private set; }
        public bool IsPrimary { get; private set; }

        private Image() { }

        public Image(string publicId, string url, string secureUrl)
        {
            PublicId = publicId;
            Url = url;
            SecureUrl = secureUrl;
        }

        public void SetEntityId(Guid entityId) => EntityId = entityId;

        public void MarkDeleted()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}