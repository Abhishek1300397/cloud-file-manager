using Amazon.S3;
using Amazon.S3.Model;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.Configuration;
using CloudStorage.Application.DTOs.Responses;
using Microsoft.Extensions.Options;

namespace CloudStorage.Infrastructure.Storage.S3
{
    internal class S3FileStorageService(IAmazonS3 s3Client, IOptions<AwsOptions> options) : IFileStorageService
    {
        public async Task<FileUploadResponse> UploadAsync(Stream stream, string fileName, 
                                                            string contentType, long size, string userId, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);

            var objectKey = $"users/{userId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = objectKey,
                InputStream = stream,
                AutoCloseStream = false,
                ContentType = contentType
            };
            request.Headers.ContentLength = size;

            await s3Client.PutObjectAsync(request, cancellationToken);

            return new FileUploadResponse(fileName, objectKey, contentType, size);
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Object key cannot be empty.", nameof(key));

            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = key
            }, cancellationToken);
        }
    }
}
