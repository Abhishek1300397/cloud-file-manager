using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Infrastructure.Persistence.Exception;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace CloudStorage.Infrastructure.Persistence
{
    public sealed class UnitOfWork(ApplicationDbContext dbContext, DatabaseExceptionTranslator exceptionTranslator) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw exceptionTranslator.Translate(exception);
            }
        }
    }
}
