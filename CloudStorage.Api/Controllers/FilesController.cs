using CloudStorage.Api.Contracts.Files;
using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController(IFileService fileService, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var file = request.File;

            await using var stream = new MemoryStream();

            await file.CopyToAsync(stream, cancellationToken);

            stream.Position = 0;

            var safeFileName = FileNameSanitizer.Sanitize(file.FileName);

            var fileCommand = new UploadFileCommand(stream, safeFileName, file.ContentType, file.Length, userId);

            var result = await fileService.UploadAsync(fileCommand, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] FileListRequest request, CancellationToken cancellationToken)
        {
            if (request.Page < 1)
                return BadRequest("Page must be greater than 0.");

            if (request.PageSize < 1 || request.PageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            var userId = currentUser.UserId;

            var files = await fileService.GetFilesAsync(userId, request.Search, request.Page, request.PageSize, cancellationToken);

            return Ok(files);
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var result = await fileService.DownloadAsync(id, userId, cancellationToken);

            return File(result.Stream, result.ContentType, result.FileName, enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            await fileService.DeleteAsync(id, userId, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id:guid}/rename")]
        public async Task<IActionResult> Rename(Guid id, [FromBody] RenameFileRequest request, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            await fileService.RenameAsync(
                id,
                userId,
                request,
                cancellationToken);

            return NoContent();
        }

        [HttpGet("/api/files/{fileId:guid}")]
        public async Task<IActionResult> GetFileMetadata(Guid fileId, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var files = await fileService.GetFileMetadataAsync(fileId, userId, cancellationToken);

            return Ok(files);
        }

        [HttpPost("presigned-upload")]
        public async Task<IActionResult> GeneratePresignedUpload([FromBody] CreatePresignedUploadRequest request, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var response = await fileService.GeneratePresignedUploadAsync(request, userId, cancellationToken);

            return Ok(response);
        }

        [HttpPost("{fileId:guid}/complete")]
        public async Task<IActionResult> CompleteUpload(Guid fileId, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            await fileService.CompletePresignedUploadAsync(fileId, userId, cancellationToken);

            return NoContent();
        }

        [HttpGet("{fileId:guid}/download-url")]
        public async Task<IActionResult> GeneratePresignedDownloadUrl(Guid fileId, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var downloadUrl = await fileService.GeneratePresignedDownloadAsync(fileId, userId, cancellationToken);

            return Ok(new
            {
                downloadUrl
            });
        }
    }
}
