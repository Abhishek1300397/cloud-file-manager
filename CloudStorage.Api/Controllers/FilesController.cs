using CloudStorage.Api.Contracts.Files;
using CloudStorage.Application.Abstractions.Files;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController(IFileService fileService) : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var file = request.File;

            if (request.File.Length == 0) return BadRequest("File cannot be empty.");

            await using var stream = new MemoryStream();

            await file.CopyToAsync(stream, cancellationToken);

            stream.Position = 0;

            var safeFileName = FileNameSanitizer.Sanitize(file.FileName);

            var fileCommand = new UploadFileCommand(stream, safeFileName, file.ContentType, file.Length, userId);

            var result = await fileService.UploadAsync(fileCommand, cancellationToken);

            return CreatedAtAction(nameof(Upload), result);
        }
    }
}
