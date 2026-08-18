namespace CloudStorage.Application.Abstractions.Caching
{
    public static class CacheKeys
    {
        public static string FileMetadata(Guid userId, Guid fileId)
            => $"file-metadata:{userId}:{fileId}";

        public static string StorageUsage(Guid userId)
            => $"storage-usage:{userId}";

    }
}
