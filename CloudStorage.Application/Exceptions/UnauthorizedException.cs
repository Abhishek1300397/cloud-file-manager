using CloudStorage.Application.Common.Enums;

namespace CloudStorage.Application.Exceptions
{
    public class UnauthorizedException(string message) : AppException(message)
    {
        public override string Title => "Un-Authroized Error";

        public override ErrorType ErrorType => ErrorType.Unauthorized;
    }
}
