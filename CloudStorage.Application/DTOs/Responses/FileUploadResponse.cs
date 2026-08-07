namespace CloudStorage.Application.DTOs.Responses
{
    public sealed record FileUploadResponse(string OriginalFileName, string ObjectKey, string ContentType, long Size);
}
