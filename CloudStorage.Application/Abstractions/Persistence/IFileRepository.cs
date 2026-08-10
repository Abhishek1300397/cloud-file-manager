using CloudStorage.Domain.Entities;

namespace CloudStorage.Application.Abstractions.Persistence
{
    public interface IFileRepository
    {
        Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);

        Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
