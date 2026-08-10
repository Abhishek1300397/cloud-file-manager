using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.Infrastructure.Persistence.Repositories
{
    internal class FileRepository(ApplicationDbContext dbContext) : IFileRepository
    {
        public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default)
        {

            await dbContext.StoredFiles.AddAsync(file, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return dbContext.StoredFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
