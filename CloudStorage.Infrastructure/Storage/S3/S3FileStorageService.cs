using Amazon.S3;
using Amazon.S3.Model;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.Configuration;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Application.DTOs.Storage;
using Microsoft.Extensions.Options;
using System.Net;

namespace CloudStorage.Infrastructure.Storage.S3
{
    internal class S3FileStorageService(IAmazonS3 s3Client, IOptions<AwsOptions> options) : IFileStorageService
    {

        public async Task<FileUploadResponse> UploadAsync(UploadFileCommand fileCommand, CancellationToken cancellationToken = default)
        {
            var fileName = (fileCommand.FileName);
            var extension = Path.GetExtension(fileName);

            var objectKey = $"users/{fileCommand.UserId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = objectKey,
                InputStream = fileCommand.Stream,
                AutoCloseStream = false,
                ContentType = fileCommand.ContentType
            };
            var size = fileCommand.Size;
            request.Headers.ContentLength = size;

            await s3Client.PutObjectAsync(request, cancellationToken);

            return new FileUploadResponse(fileName, objectKey, fileCommand.ContentType, size);
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

        public async Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(objectKey)) throw new ArgumentException("Object key cannot be empty.", nameof(objectKey));

            var response = await s3Client.GetObjectAsync(
                                new GetObjectRequest
                                {
                                    BucketName = options.Value.BucketName,
                                    Key = objectKey
                                },
                                cancellationToken);

            return response.ResponseStream;
        }

        public Task<string> GenerateUploadUrlAsync(string objectKey, string contentType, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.Value.BucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = s3Client.GetPreSignedURL(request);

            return Task.FromResult(url);
        }

        public Task<string> GenerateDownloadUrlAsync(string objectKey, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.Value.BucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = s3Client.GetPreSignedURL(request);

            return Task.FromResult(url);
        }

        public async Task<StorageObjectMetadata?> GetMetadataAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = options.Value.BucketName,
                    Key = objectKey
                };

                var response = await s3Client.GetObjectMetadataAsync(request, cancellationToken);

                return new StorageObjectMetadata(response.ContentLength, response.Headers.ContentType);
            }
            catch (AmazonS3Exception exception)
                when (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
