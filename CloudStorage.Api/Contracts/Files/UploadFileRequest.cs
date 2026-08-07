namespace CloudStorage.Api.Contracts.Files
{
    public sealed class UploadFileRequest
    {
        public required IFormFile File { get; init; }
    }
}
