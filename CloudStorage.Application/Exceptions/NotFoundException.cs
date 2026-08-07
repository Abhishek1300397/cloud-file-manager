using CloudStorage.Application.Common.Enums;

namespace CloudStorage.Application.Exceptions
{
    public class NotFoundException(string message) : AppException(message)
    {
        public override string Title => "Resource Not Found";

        public override ErrorType ErrorType => ErrorType.NotFound;
    }
}
