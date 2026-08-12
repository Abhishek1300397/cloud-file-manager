using CloudStorage.Application.DTOs.Requests;
using FluentValidation;

namespace CloudStorage.Application.Validators.Files
{
    public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        private static readonly string[] AllowedExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf",
            ".txt",
            ".doc",
            ".docx"
        ];

        private static readonly string[] AllowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "application/pdf",
            "text/plain",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        ];

        public UploadFileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");

            RuleFor(x => x.Size)
                .GreaterThan(0)
                .WithMessage("File cannot be empty.")
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("File size cannot exceed 10 MB.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(type => AllowedContentTypes.Contains(
                    type,
                    StringComparer.OrdinalIgnoreCase))
                .WithMessage("File type is not supported.");

            RuleFor(x => x.FileName)
                .Must(HasAllowedExtension)
                .WithMessage("File extension is not supported.");

        }

        private static bool HasAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
    }
}
