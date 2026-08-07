using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Application.Abstractions.Security;
using CloudStorage.Application.DTOs.Requests;
using CloudStorage.Application.DTOs.Responses;
using CloudStorage.Application.Exceptions;
using CloudStorage.Domain.Entities;

namespace CloudStorage.Application.Services;

public sealed class AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) ?? throw new UnauthorizedException("Invalid email or password.");

        var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid) throw new UnauthorizedException("Invalid email or password.");

        var accessToken = jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(user.Id, user.Email, accessToken);
    }

    public async Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Create(request.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}