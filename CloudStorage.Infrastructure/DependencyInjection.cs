using Amazon;
using Amazon.S3;
using CloudStorage.Application.Abstractions.Caching;
using CloudStorage.Application.Abstractions.Persistence;
using CloudStorage.Application.Abstractions.Security;
using CloudStorage.Application.Abstractions.Storage;
using CloudStorage.Application.Configuration;
using CloudStorage.Infrastructure.Caching;
using CloudStorage.Infrastructure.Persistence;
using CloudStorage.Infrastructure.Persistence.Exception;
using CloudStorage.Infrastructure.Persistence.Repositories;
using CloudStorage.Infrastructure.Security;
using CloudStorage.Infrastructure.Storage.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CloudStorage.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL") ?? throw new InvalidOperationException("PostgreSQL connection string is not configured.");

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            var redisConnection = configuration[$"{RedisOptions.SectionName}:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new InvalidOperationException("Redis is not configured.");
            }
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));

            services.AddSingleton<DatabaseExceptionTranslator>();

            services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IFileRepository, FileRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IFileStorageService, S3FileStorageService>();

            services.AddScoped<ICacheService, RedisCacheService>();

            var region = configuration[$"{AwsOptions.SectionName}:Region"];

            if (string.IsNullOrWhiteSpace(region))
            {
                throw new InvalidOperationException("AWS Region is not configured.");
            }
            var endpoint = RegionEndpoint.GetBySystemName(region);

            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(endpoint));

            return services;
        }


    }
}
