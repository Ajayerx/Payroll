using Microsoft.Extensions.Caching.Memory;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, int minutes = 10)
    {
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
    }

    public T GetOrSet<T>(string key, Func<T> factory, int minutes = 10)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
            return cachedValue!;

        var value = factory();
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
        return value;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int minutes = 10)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
            return cachedValue!;

        var value = await factory();
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void Clear()
    {
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }
    }
}
