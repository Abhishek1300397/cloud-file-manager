using CloudStorage.Application.Common.Enums;

namespace CloudStorage.Application.Exceptions
{
    public class ValidationException(string message) : AppException(message)
    {
        public override string Title => "Validation Error";

        public override ErrorType ErrorType => ErrorType.Validation;
    }
}
