using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Services;

public class CachingService : ICachingService
{
    private readonly TimeSpan _cachDefaultDuration = TimeSpan.FromMinutes(30);
    private readonly IMemoryCache _cache;

    private readonly ILogger<CachingService>? _logger;

    public CachingService(IMemoryCache cache, ILogger<CachingService>? logger = null)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrAddCacheAsync<T>(Func<Task<T>> fetchFunction, bool refreshCache = false
        , TimeSpan? cacheDuration = null, string? uniqueIdentity = null)
    {
        string cacheKey = uniqueIdentity == null
            ? $"{typeof(T).Name}_Get_{fetchFunction.Method.Name}"
            : $"{typeof(T).Name}_Get_{fetchFunction.Method.Name}" + $"_Id_{uniqueIdentity}";

        if (refreshCache)
        {
            _cache.Remove(cacheKey);
        }
        else
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedResult))
            {
                return cachedResult;
            }
        }

        T? result = await fetchFunction();
        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheDuration ?? _cachDefaultDuration);

        _ = _cache.Set(cacheKey, result, cacheEntryOptions);

        return result;
    }
}