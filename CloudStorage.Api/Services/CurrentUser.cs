using CloudStorage.Application.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CloudStorage.Api.Services
{
    public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        public Guid UserId
        {
            get
            {
                var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(userId, out var id)
                    ? id
                    : throw new UnauthorizedAccessException(
                        "User is not authenticated.");
            }
        }

        public string? Email =>
            httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.Email);
    }
}
