using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;

namespace CloudStorage.Application.Abstractions.Authentication
{
    public interface IAuthService
    {
        Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
