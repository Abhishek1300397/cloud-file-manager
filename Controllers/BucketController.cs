using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace AWS_S3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BucketController(IAmazonS3 s3Client) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (bucketExist)
                return BadRequest($"Bucket {bucketName} Already exist");
            await s3Client.PutBucketAsync(bucketName);
            return Created("buckets", $"Bucket {bucketName} Created Successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBucketAsync()
        {
            var data = await s3Client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => b.BucketName).ToList();
            return Ok(buckets);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName)
        {
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExist)
                return BadRequest($"Bucket {bucketName} does not exist");
            await s3Client.DeleteBucketAsync(bucketName);
            return Ok($"Bucket {bucketName} Deleted Successfully");
        }
    }
}
