using CloudStorage.Domain.Entities;

namespace CloudStorage.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);


        Task AddAsync(User user, CancellationToken cancellationToken = default);
    }
}
