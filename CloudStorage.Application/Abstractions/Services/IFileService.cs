using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;

namespace CloudStorage.Application.Abstractions.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default);

        Task<FileDownloadResponse> DownloadAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);

        Task<PagedResponse<FileListItemResponse>> GetFilesAsync(Guid userId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);

        Task RenameAsync(Guid fileId, Guid userId, RenameFileRequest fileName, CancellationToken cancellationToken = default);

        Task<FileDeatails> GetFileMetaDataAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}
