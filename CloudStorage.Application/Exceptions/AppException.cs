using CloudStorage.Application.Common.Enums;

namespace CloudStorage.Application.Exceptions
{
    public abstract class AppException(string message) : Exception(message)
    {
        public abstract ErrorType ErrorType { get; }

        public abstract string Title { get; }
    }
}
