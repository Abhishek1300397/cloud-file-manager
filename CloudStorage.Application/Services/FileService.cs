using CloudStorage.Application.Abstractions.Files;
using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace CloudStorage.Application.Services
{
    public class FileService(IFileStorageService fileStorageService, IValidator<UploadFileCommand> validator, IFileSignatureValidator fileSignatureValidator, IFileRepository fileRepository, ILogger<FileService> logger) : IFileService
    {

        public async Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(fileCommand, cancellationToken);

            var isValidSignature = await fileSignatureValidator.IsValidAsync(fileCommand.Stream, fileCommand.FileName, cancellationToken);

            if (!isValidSignature) throw new ValidationException([new ValidationFailure(nameof(fileCommand.FileName), "File content does not match the file extension.")]);

            var uploadResult = await fileStorageService.UploadAsync(fileCommand.Stream, fileCommand.FileName, fileCommand.ContentType, fileCommand.Size, fileCommand.UserId, cancellationToken);

            var storedFile = new StoredFile(fileCommand.UserId, uploadResult.OriginalFileName, uploadResult.ObjectKey, uploadResult.ContentType, uploadResult.Size);

            try
            {
                await fileRepository.AddAsync(storedFile, cancellationToken);
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
    }
}
