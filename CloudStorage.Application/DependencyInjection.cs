using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication( this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
