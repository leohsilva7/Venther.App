using System;
using System.Threading.Tasks;
using Venther.App.Models;

namespace Venther.App.Services.Interfaces;

public interface ICacheService
{
    Task SaveAsync<T>(string key, T value);
    Task<CacheEntry<T>?> LoadAsync<T>(string key);
    Task ClearAllAsync();
}
