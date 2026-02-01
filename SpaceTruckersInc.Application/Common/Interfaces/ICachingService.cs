namespace SpaceTruckersInc.Application.Common.Interfaces;

public interface ICachingService
{
    Task<T?> GetOrAddCacheAsync<T>(Func<Task<T>> fetchFunction, bool refreshCache = false, TimeSpan? cacheDuration = null
        , string? uniqueIdentity = null);
}