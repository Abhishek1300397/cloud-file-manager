using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;

namespace CloudStorage.Application.Abstractions.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default);
    }
}
