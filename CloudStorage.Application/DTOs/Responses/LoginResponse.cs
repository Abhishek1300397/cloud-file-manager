namespace CloudStorage.Application.DTOs.Responses
{
    public sealed record LoginResponse(Guid UserId,string Email,string AccessToken);
}
