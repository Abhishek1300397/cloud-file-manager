using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
namespace CloudStorage.Application.Abstractions.Storage
{
    public interface IFileStorageService
    {
        Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default);

        Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

        Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);
    }
}
