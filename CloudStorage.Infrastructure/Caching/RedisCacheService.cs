using CloudStorage.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CloudStorage.Infrastructure.Caching
{
    internal sealed class RedisCacheService(IDistributedCache distributedCache,
    IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await distributedCache.GetStringAsync(key, cancellationToken);

                if (string.IsNullOrWhiteSpace(cachedValue)) return default;

                return JsonSerializer.Deserialize<T>(cachedValue, JsonOptions);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis cache read failed for key {CacheKey}. Falling back to source.", key);

                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, JsonOptions);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                await distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis cache failed to set key {CacheKey}.", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await distributedCache.RemoveAsync(key, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis cache failed to del key {CacheKey}.", key);
            }
        }


        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);

            if (cachedValue is not null)
                return cachedValue;

            var lockKey = $"cache-lock:{key}";
            var lockValue = Guid.NewGuid().ToString();
            var database = redis.GetDatabase();

            var acquired = false;

            try
            {
                try
                {
                    acquired = await database.StringSetAsync(lockKey, lockValue, TimeSpan.FromSeconds(10), When.NotExists);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Redis cache lock acquisition failed for key {CacheKey}. Falling back to source.", key);

                    return await factory(cancellationToken);
                }

                if (acquired)
                {
                    // Double-check after acquiring the lock.
                    cachedValue = await GetAsync<T>(key, cancellationToken);

                    if (cachedValue is not null) return cachedValue;

                    var value = await factory(cancellationToken);

                    await SetAsync(key, value, expiration, cancellationToken);

                    return value;
                }

                // Another request owns the lock.
                // Wait for it to populate the cache.
                const int maxAttempts = 50;

                for (var attempt = 0; attempt < maxAttempts; attempt++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

                    cachedValue = await GetAsync<T>(key, cancellationToken);

                    if (cachedValue is not null) return cachedValue;

                    // The original lock may have expired.
                    // Try to acquire it ourselves.
                    try
                    {
                        acquired = await database.StringSetAsync(lockKey, lockValue, TimeSpan.FromSeconds(10), When.NotExists);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,"Redis cache lock retry failed for key {CacheKey}. Falling back to source.",key);
                        return await factory(cancellationToken);
                    }

                    if (acquired)
                    {
                        cachedValue = await GetAsync<T>(key, cancellationToken);

                        if (cachedValue is not null) return cachedValue;

                        var value = await factory(cancellationToken);

                        await SetAsync(key, value, expiration, cancellationToken);

                        return value;
                    }
                }

                // Cache could not be populated within the bounded wait.
                // Preserve availability by falling back to the source.
                return await factory(cancellationToken);
            }
            finally
            {
                if (acquired)
                {
                    try
                    {
                        await ReleaseLockAsync(database, lockKey, lockValue);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to release Redis cache lock {LockKey}.", lockKey);
                    }
                }
            }
        }

        private static async Task ReleaseLockAsync(IDatabase database, RedisKey lockKey, RedisValue lockValue)
        {
            const string script = """
                                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                                        return redis.call('DEL', KEYS[1])
                                    end
                                    return 0
                                    """;

            await database.ScriptEvaluateAsync(script, [lockKey], [lockValue]);
        }
    }
}
