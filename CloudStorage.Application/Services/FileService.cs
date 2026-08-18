using CloudStorage.Application.Abstractions.Caching;
using CloudStorage.Application.Abstractions.Files;
using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.Configuration;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Application.Exceptions;
using CloudStorage.Domain.Entities;
using CloudStorage.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudStorage.Application.Services
{
    public class FileService(IFileStorageService fileStorageService,
                                IValidator<UploadFileCommand> validator,
                                IValidator<RenameFileRequest> renameFileValidator,
                                IValidator<CreatePresignedUploadRequest> presignedUploadValidator,
                                IFileSignatureValidator fileSignatureValidator,
                                IFileRepository fileRepository,
                                ILogger<FileService> logger,
                                IUnitOfWork unitOfWork,
                                ICacheService cacheService,
                                IOptions<AwsOptions> awsOptions) : IFileService
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

            var storedFile = new StoredFile(Guid.NewGuid(), fileCommand.UserId, uploadResult.OriginalFileName, uploadResult.ObjectKey, uploadResult.ContentType, uploadResult.Size);

            storedFile.MarkAsUploaded();

            try
            {
                await fileRepository.AddAsync(storedFile, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                await cacheService.RemoveAsync(CacheKeys.StorageUsage(fileCommand.UserId), cancellationToken);

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

            await cacheService.RemoveAsync(CacheKeys.FileMetadata(userId, fileId), cancellationToken);

            await cacheService.RemoveAsync(CacheKeys.StorageUsage(userId), cancellationToken);
        }

        public async Task RenameAsync(Guid fileId, Guid userId, RenameFileRequest fileRequest, CancellationToken cancellationToken = default)
        {
            await renameFileValidator.ValidateAndThrowAsync(fileRequest, cancellationToken);

            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId) throw new ForbiddenException("You do not have access to this file.");

            storedFile.Rename(FileNameSanitizer.Sanitize(fileRequest.FileName));

            await fileRepository.UpdateAsync(storedFile, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheService.RemoveAsync(CacheKeys.FileMetadata(userId, fileId), cancellationToken);
        }

        public async Task<FileMetadataResponse> GetFileMetadataAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.FileMetadata(userId, fileId);

            return await cacheService.GetOrCreateAsync(cacheKey,
                async token =>
                {
                    var storedFile = await fileRepository.GetByIdAsync(fileId, token) ?? throw new NotFoundException("File not found.");

                    if (storedFile.UserId != userId) throw new ForbiddenException("You do not have access to this file.");

                    return new FileMetadataResponse
                    {
                        Id = storedFile.Id,
                        FileName = storedFile.OriginalFileName,
                        ContentType = storedFile.ContentType,
                        Size = storedFile.Size,
                        CreatedAt = storedFile.CreatedAtUtc
                    };
                },
                TimeSpan.FromMinutes(10), cancellationToken);
        }

        public async Task<PresignedUploadResponse> GeneratePresignedUploadAsync(CreatePresignedUploadRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            await presignedUploadValidator.ValidateAndThrowAsync(request, cancellationToken);

            var fileId = Guid.NewGuid();

            string fileName = FileNameSanitizer.Sanitize(request.FileName);

            var extension = Path.GetExtension(fileName);

            var objectKey = $"users/{userId}/{DateTime.UtcNow:yyyy/MM}/{fileId}{extension}";

            var expiration = TimeSpan.FromMinutes(awsOptions.Value.PresignedUploadExpirationMinutes);

            var expiresAtUtc = DateTime.UtcNow.Add(expiration);

            var storedFile = new StoredFile(fileId, userId, fileName, objectKey, request.ContentType, request.Size);

            await fileRepository.AddAsync(storedFile, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var uploadUrl = await fileStorageService.GenerateUploadUrlAsync(objectKey, request.ContentType, expiration, cancellationToken);

            return new PresignedUploadResponse
            {
                FileId = fileId,
                UploadUrl = uploadUrl,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public async Task CompletePresignedUploadAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId) 
                throw new ForbiddenException("You do not have access to this file.");

            if (storedFile.Status != FileStatus.Pending) 
                throw new InvalidOperationException("File upload has already been completed.");

            var metadata = await fileStorageService.GetMetadataAsync(storedFile.ObjectKey, cancellationToken) ?? 
                            throw new NotFoundException("Uploaded file was not found in storage.");

            if (metadata.ContentLength != storedFile.Size) 
                throw new Exceptions.ValidationException("Uploaded file size does not match the expected size.");

            if (!string.Equals(metadata.ContentType, storedFile.ContentType, StringComparison.OrdinalIgnoreCase)) 
                throw new Exceptions.ValidationException("Uploaded file content type does not match the expected content type.");

            storedFile.MarkAsUploaded();

            await fileRepository.UpdateAsync(storedFile, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheService.RemoveAsync(CacheKeys.StorageUsage(userId), cancellationToken);
        }

        public async Task<string> GeneratePresignedDownloadAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            var storedFile = await fileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new NotFoundException("File not found.");

            if (storedFile.UserId != userId) throw new ForbiddenException("You do not have access to this file.");

            if (storedFile.Status != FileStatus.Uploaded) throw new InvalidOperationException("File has not been uploaded yet.");

            var expiration = TimeSpan.FromMinutes(awsOptions.Value.PresignedDownloadExpirationMinutes);

            var downloadUrl = await fileStorageService.GenerateDownloadUrlAsync(storedFile.ObjectKey, expiration, cancellationToken);

            return downloadUrl;
        }

        public async Task<StorageUsageResponse> GetStorageUsageAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.StorageUsage(userId);

            var result = await cacheService.GetOrCreateAsync(cacheKey,
                async token =>
                {
                    var (fileCount, totalSize) = await fileRepository.GetStorageUsageAsync(userId, token);

                    return new StorageUsageResponse
                    {
                        FileCount = fileCount,
                        TotalSize = totalSize
                    };
                },
                TimeSpan.FromMinutes(10), cancellationToken);

            return result;
        }
    }
}
