using CloudStorage.Application.Common.Enums;

namespace CloudStorage.Application.Exceptions
{
    public class ForbiddenException(string message) : AppException(message)
    {
        public override ErrorType ErrorType => ErrorType.Forbidden;

        public override string Title => "Forbidden Error";
    }
}
