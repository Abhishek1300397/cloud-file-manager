using CloudStorage.Domain.Entities;

namespace CloudStorage.Application.Abstractions.Security
{

    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
