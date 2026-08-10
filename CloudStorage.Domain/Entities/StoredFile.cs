namespace CloudStorage.Domain.Entities
{
    public sealed class StoredFile
    {
        public Guid Id { get; private set; }

        public string UserId { get; private set; } = null!;

        public string OriginalFileName { get; private set; } = null!;

        public string ObjectKey { get; private set; } = null!;

        public string ContentType { get; private set; } = null!;

        public long Size { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        private StoredFile()
        {
        }

        public StoredFile(
            string userId,
            string originalFileName,
            string objectKey,
            string contentType,
            long size)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            OriginalFileName = originalFileName;
            ObjectKey = objectKey;
            ContentType = contentType;
            Size = size;
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
