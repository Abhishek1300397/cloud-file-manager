namespace CloudStorage.Application.DTOs.Responses
{
    public sealed class StorageUsageResponse
    {
        public long FileCount { get; init; }

        public long TotalSize { get; init; }
    }
}
