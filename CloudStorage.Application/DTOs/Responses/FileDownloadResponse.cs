namespace CloudStorage.Application.DTOs.Responses
{
    public sealed class FileDownloadResponse
    {
        public required Stream Stream { get; init; }
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public long Size { get; init; }
    }
}
