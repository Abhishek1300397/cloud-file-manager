namespace CloudStorage.Domain.Entities
{
    public sealed class StoredFile
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string OriginalFileName { get; private set; } = null!;

        public string ObjectKey { get; private set; } = null!;

        public string ContentType { get; private set; } = null!;

        public long Size { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public User User { get; private set; } = null!;

        private StoredFile()
        {
        }

        public StoredFile(
            Guid userId,
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

        public void Rename(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(
                    "File name cannot be empty.",
                    nameof(fileName));

            OriginalFileName = fileName;
        }
    }
}
