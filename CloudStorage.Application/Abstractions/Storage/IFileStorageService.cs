using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Application.DTOs.Storage;
namespace CloudStorage.Application.Abstractions.Storage
{
    public interface IFileStorageService
    {
        Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default);

        Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

        Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);

        Task<string> GenerateUploadUrlAsync(string objectKey, string contentType, TimeSpan expiration, CancellationToken cancellationToken = default);

        Task<string> GenerateDownloadUrlAsync(string objectKey, TimeSpan expiration, CancellationToken cancellationToken = default);

        Task<StorageObjectMetadata?> GetMetadataAsync(string objectKey,CancellationToken cancellationToken = default);

    }
}
