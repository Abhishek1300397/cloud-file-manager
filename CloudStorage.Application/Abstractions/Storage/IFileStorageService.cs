using CloudStorage.Application.DTOs.Responses;
namespace CloudStorage.Application.Abstractions.Storage
{
    public interface IFileStorageService
    {
        Task<FileUploadResponse> UploadAsync(Stream stream, string fileName, string contentType, long size, string userId,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
    }
}
