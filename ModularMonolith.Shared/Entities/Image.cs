namespace ModularMonolith.Shared.Entities
{
    public class Image : BaseEntity
    {
        public Guid? EntityId { get; private set; }
        public string PublicId { get; private set; } = null!;
        public string Url { get; private set; } = null!;
        public string SecureUrl { get; private set; } = null!;
        public bool IsPrimary { get; private set; }

        private Image() { }

        public Image(string publicId, string url, string secureUrl)
        {
            PublicId = publicId;
            Url = url;
            SecureUrl = secureUrl;
        }

        public void SetEntityId(Guid entityId) => EntityId = entityId;
    }
}