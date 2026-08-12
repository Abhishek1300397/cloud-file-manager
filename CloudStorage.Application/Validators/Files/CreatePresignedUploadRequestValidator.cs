using CloudStorage.Application.DTOs.Requests;
using FluentValidation;

namespace CloudStorage.Application.Validators.Files
{
    public sealed class CreatePresignedUploadRequestValidator : AbstractValidator<CreatePresignedUploadRequest>
    {
        private const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
        [
            "application/pdf",
            "image/jpeg",
            "image/png",
            "text/plain",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        ];

        public CreatePresignedUploadRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.")
                .MaximumLength(255)
                .WithMessage("File name cannot exceed 255 characters.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(IsAllowedContentType)
                .WithMessage("Unsupported content type.");

            RuleFor(x => x.Size)
                .GreaterThan(0)
                .WithMessage("File cannot be empty.")
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("File size cannot exceed 10 MB.");
        }

        private static bool IsAllowedContentType(string contentType)
        {
            return AllowedContentTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
