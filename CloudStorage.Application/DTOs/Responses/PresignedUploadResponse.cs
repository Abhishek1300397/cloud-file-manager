namespace CloudStorage.Application.DTOs.Responses
{
    public class PresignedUploadResponse
    {
        public Guid FileId { get; init; }

        public string UploadUrl { get; init; } = string.Empty;

        public DateTime ExpiresAtUtc { get; init; }
    }
}
