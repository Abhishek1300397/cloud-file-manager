using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var userId = await authService.RegisterAsync(request, cancellationToken);

            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await authService.LoginAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
    }
}