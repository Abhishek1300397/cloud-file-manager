using CloudStorage.Application.Common.Enums;
using System.Net;

namespace CloudStorage.Application.Exceptions
{
    public class ConflictException(string message) : AppException(message)
    {
        public override string Title => "Conflict Error";

        public override ErrorType ErrorType => ErrorType.Conflict;
    }
}
