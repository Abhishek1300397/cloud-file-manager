using CloudStorage.Api.Contracts.Files;
using CloudStorage.Application.Abstractions.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController(IFileStorageService fileStorageService) : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var file = request.File;

            await using var stream = file.OpenReadStream();

            var result = await fileStorageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                userId,
                cancellationToken);

            return Ok(result);
        }
    }
}
