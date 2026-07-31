using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace AWS_S3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(IAmazonS3 s3Client) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExist)
                return BadRequest($"Bucket {bucketName} does not exist");
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await s3Client.PutObjectAsync(request);
            return Ok($"File {file.FileName} Uploaded Successfully to Bucket {bucketName}");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync(string bucketName, string? prefix)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExist)
                return BadRequest($"Bucket {bucketName} does not exist");
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await s3Client.ListObjectsV2Async(request);
            var s3Object = result.S3Objects.Select(o => new Models.S3ObjectDto
            {
                Name = o.Key,
                PresignedUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = o.Key,
                    Expires = DateTime.UtcNow.AddMinutes(5)
                })
            }).ToList();
            return Ok(s3Object);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string bucketName, string fileName)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExist)
                return BadRequest($"Bucket {bucketName} does not exist");
            await s3Client.DeleteObjectAsync(bucketName, fileName);
            return Ok($"File {fileName} Deleted Successfully from Bucket {bucketName}");

        }

        [HttpGet("Preview")]
        public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExist)
                return BadRequest($"Bucket {bucketName} does not exist");
            var s3Object = await s3Client.GetObjectAsync(bucketName, key);
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType, s3Object.Key);
        }


    }
}
