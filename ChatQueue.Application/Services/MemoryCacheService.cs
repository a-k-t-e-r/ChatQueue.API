using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatQueue.Application.Services;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> TryGetValueAsync<T>(string key, out T value);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
}

public class MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger) : ICacheService
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<MemoryCacheService> _logger = logger;

    public Task<T> GetAsync<T>(string key)
    {
        try
        {
            var value = _cache.Get<T>(key);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key {Key}", key);
            throw;
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }
            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key {Key}", key);
            throw;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key {Key}", key);
            throw;
        }
    }

    public Task<bool> TryGetValueAsync<T>(string key, out T value)
    {
        try
        {
            var result = _cache.TryGetValue(key, out value);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to get cache value for key {Key}", key);
            throw;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        try
        {
            if (!_cache.TryGetValue(key, out T value))
            {
                // Key not in cache, get the data from factory
                value = await factory();

                var options = new MemoryCacheEntryOptions();
                if (expiry.HasValue)
                {
                    options.SetAbsoluteExpiration(expiry.Value);
                }
                _cache.Set(key, value, options);
            }
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key {Key}", key);
            throw;
        }
    }
}