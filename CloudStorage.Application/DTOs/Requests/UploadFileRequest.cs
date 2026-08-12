namespace CloudStorage.Application.DTOs.Requests
{
    public sealed record UploadFileCommand(Stream Stream, string FileName, string ContentType, long Size, Guid UserId);
}
