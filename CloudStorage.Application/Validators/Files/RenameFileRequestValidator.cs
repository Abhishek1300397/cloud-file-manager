using CloudStorage.Application.DTOs.Requests;
using FluentValidation;

namespace CloudStorage.Application.Validators.Files
{
    public sealed class RenameFileRequestValidator: AbstractValidator<RenameFileRequest>
    {
        public RenameFileRequestValidator()
        {
            RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        }
    }
}
