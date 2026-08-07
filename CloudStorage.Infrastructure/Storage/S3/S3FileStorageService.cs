using Amazon.S3;
using Amazon.S3.Model;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.Configuration;
using CloudStorage.Application.DTOs.Responses;

namespace CloudStorage.Infrastructure.Storage.S3
{
    internal class S3FileStorageService(IAmazonS3 s3Client, AwsOptions options) : IFileStorageService
    {
        public async Task<FileUploadResponse> UploadAsync(Stream stream, string fileName, string contentType, long size, string userId, CancellationToken cancellationToken = default)
        {
            var response = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = "myfile-manager",
                MaxKeys = 1
            });

            Console.WriteLine($"Objects returned: {response.KeyCount}");

            var extension = Path.GetExtension(fileName);

            var objectKey = $"users/{userId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = options.BucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType
            };

            await s3Client.PutObjectAsync(request, cancellationToken);

            return new FileUploadResponse(fileName, objectKey, contentType, size);
        }
    }
}
