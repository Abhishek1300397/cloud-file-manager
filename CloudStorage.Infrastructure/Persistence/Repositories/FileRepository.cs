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
        }

        public async Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default)
        {
            dbContext.StoredFiles.Remove(file);

        }

        public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return dbContext.StoredFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }


        public async Task<(IReadOnlyList<StoredFile> Items, long TotalCount)> GetByUserIdAsync(Guid userId, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = dbContext.StoredFiles.AsNoTracking().Where(x => x.UserId == userId);


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => EF.Functions.ILike(x.OriginalFileName, $"%{search}%"));
            }


            var totalCount = await query.LongCountAsync(cancellationToken);

            var items = await query
                            .OrderByDescending(x => x.CreatedAtUtc)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default)
        {
            dbContext.StoredFiles.Update(file);
        }
    }
}
