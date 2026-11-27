using System;

namespace Venther.App.Models;

public record CacheEntry<T>(T Data, DateTime SavedAt);

public record CachedResult<T>(T Data, bool FromCache, DateTime? CachedAt = null)
{
    public static CachedResult<T> Fresh(T data) => new(data, false, null);
    public static CachedResult<T> FromStorage(T data, DateTime cachedAt) => new(data, true, cachedAt);
}
