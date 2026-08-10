using CloudStorage.Api.Middleware;
using CloudStorage.Api.Services;
using CloudStorage.Application;
using CloudStorage.Application.Abstractions.Authentication;
using CloudStorage.Application.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CloudStorage.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        services.AddValidatorsFromAssemblyContaining<AssemblyReference>();
        //   services.AddSingleton<AwsOptions>();

        return services;
    }

    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration
                        .GetSection(JwtOptions.SectionName)
                        .Get<JwtOptions>()
                        ?? throw new InvalidOperationException(
                            "JWT configuration is missing.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                        ClockSkew = TimeSpan.Zero
                    };
            });

        return services;
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    public static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {


        var connectionString = configuration.GetConnectionString("PostgreSQL") ?? throw new InvalidOperationException("PostgreSQL connection string is not configured.");

        //  var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddHealthChecks().AddNpgSql(connectionString, name: "postgresql");
        return services;
    }
}