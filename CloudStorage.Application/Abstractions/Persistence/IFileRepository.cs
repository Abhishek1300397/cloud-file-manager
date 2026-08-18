using CloudStorage.Domain.Entities;

namespace CloudStorage.Application.Abstractions.Persistence
{
    public interface IFileRepository
    {
        Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);

        Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<StoredFile> Items, long TotalCount)> GetByUserIdAsync(Guid userId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

        Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default);

        Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default);

        Task<(long FileCount, long TotalSize)> GetStorageUsageAsync(Guid userId,CancellationToken cancellationToken = default);
    }
}
