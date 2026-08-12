using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Abstractions.Files;
using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Application.Exceptions;
using CloudStorage.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace CloudStorage.Application.Services
{
    public class FileService(IFileStorageService fileStorageService,
                                IValidator<UploadFileCommand> validator,
                                IValidator<RenameFileRequest> renameFileValidator,
                                IFileSignatureValidator fileSignatureValidator,
                                IFileRepository fileRepository,
                                ILogger<FileService> logger,
                                IUnitOfWork unitOfWork) : IFileService
    {

        public async Task<FileDownloadResponse> DownloadAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId)
                throw new ForbiddenException("You do not have access to this file.");

            var stream = await fileStorageService.DownloadAsync(storedFile.ObjectKey, cancellationToken);

            return new FileDownloadResponse()
            {
                Stream = stream,
                FileName = storedFile.OriginalFileName,
                ContentType = storedFile.ContentType,
                Size = storedFile.Size
            };
        }

        public async Task<PagedResponse<FileListItemResponse>> GetFilesAsync(Guid userId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {


            var (files, totalCount) = await fileRepository.GetByUserIdAsync(userId, search, page, pageSize, cancellationToken);

            var items = files
                        .Select(file => new FileListItemResponse
                        {
                            Id = file.Id,
                            FileName = file.OriginalFileName,
                            ContentType = file.ContentType,
                            Size = file.Size,
                            CreatedAt = file.CreatedAtUtc
                        })
                        .ToList();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            return new PagedResponse<FileListItemResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(fileCommand, cancellationToken);

            var isValidSignature = await fileSignatureValidator.IsValidAsync(fileCommand.Stream, fileCommand.FileName, cancellationToken);

            if (!isValidSignature) throw new FluentValidation.ValidationException([new ValidationFailure(nameof(fileCommand.FileName), "File content does not match the file extension.")]);

            var uploadResult = await fileStorageService.UploadAsync(fileCommand, cancellationToken);

            var storedFile = new StoredFile(fileCommand.UserId, uploadResult.OriginalFileName, uploadResult.ObjectKey, uploadResult.ContentType, uploadResult.Size);

            try
            {
                await fileRepository.AddAsync(storedFile, cancellationToken);

                await unitOfWork.SaveChangesAsync(
                    cancellationToken);

            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to save file metadata for {ObjectKey}. Attempting S3 cleanup.", uploadResult.ObjectKey);
                try
                {
                    await fileStorageService.DeleteAsync(uploadResult.ObjectKey, cancellationToken);

                }
                catch (Exception cleanupException)
                {
                    logger.LogError(cleanupException, "Failed to delete S3 object {ObjectKey} after metadata persistence failed.", uploadResult.ObjectKey);
                }

                throw;
            }


            return uploadResult;
        }

        public async Task DeleteAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId) throw new ForbiddenException("You do not have access to this file.");

            await fileStorageService.DeleteAsync(storedFile.ObjectKey, cancellationToken);

            await fileRepository.DeleteAsync(storedFile, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RenameAsync(Guid fileId, Guid userId, RenameFileRequest fileRequest, CancellationToken cancellationToken = default)
        {
            await renameFileValidator.ValidateAndThrowAsync(fileRequest, cancellationToken);

            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId) throw new ForbiddenException("You do not have access to this file.");

            storedFile.Rename(FileNameSanitizer.Sanitize(fileRequest.FileName));

            await fileRepository.UpdateAsync(storedFile, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<FileDeatails> GetFileMetaDataAsync(Guid fileId, Guid userId ,CancellationToken cancellationToken = default)
        {
            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId)
                throw new ForbiddenException(
                    "You do not have access to this file.");

            return new FileDeatails
            {
                Id = storedFile.Id,
                FileName = storedFile.OriginalFileName,
                ContentType = storedFile.ContentType,
                Size = storedFile.Size,
                CreatedAt = storedFile.CreatedAtUtc
            };
        }
    }
}
