using CloudStorage.Application.Exceptions;
using CloudStorage.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CloudStorage.Infrastructure.Persistence.Exception
{
    public sealed class DatabaseExceptionTranslator
    {
        public System.Exception Translate(DbUpdateException exception)
        {
            if (exception.InnerException is not PostgresException postgresException)
                return exception;

            if (postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
                return exception;

            return postgresException.ConstraintName switch
            {
                DatabaseConstraints.UserEmailUnique =>
                    new ConflictException("A user with this email already exists."),

                _ => exception
            };
        }
        
    }
}
