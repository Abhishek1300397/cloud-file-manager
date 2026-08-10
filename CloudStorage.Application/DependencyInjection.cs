using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Abstractions.Files;
using CloudStorage.Application.Abstractions.Services;
using CloudStorage.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication( this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileSignatureValidator, FileSignatureValidator>();
            return services;
        }
    }
}
