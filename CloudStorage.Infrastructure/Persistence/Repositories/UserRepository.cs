using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudStorage.Infrastructure.Persistence.Repositories
{
    internal class UserRepository(ApplicationDbContext dbContext , IUnitOfWork unitOfWork) : IUserRepository
    {
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }
    }
}
