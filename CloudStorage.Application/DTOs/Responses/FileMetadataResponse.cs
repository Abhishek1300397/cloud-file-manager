namespace CloudStorage.Application.DTOs.Responses
{
    public class FileMetadataResponse 
    {
        public Guid Id { get; init; }

        public string FileName { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public long Size { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
