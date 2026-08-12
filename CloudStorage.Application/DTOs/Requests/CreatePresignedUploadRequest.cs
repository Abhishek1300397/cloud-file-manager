namespace CloudStorage.Application.DTOs.Requests
{
    public sealed class CreatePresignedUploadRequest
    {
        public string FileName { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public long Size { get; init; }
    }
}
