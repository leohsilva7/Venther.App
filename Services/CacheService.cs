using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class CacheService : ICacheService
{
    private readonly string _cacheDirectory;
    private static readonly SemaphoreSlim FileLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CacheService()
    {
        _cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "cache");
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    private string GetFilePath(string key)
    {
        var sanitizedKey = key.Replace(':', '_').Replace('/', '_').Replace('\\', '_');
        return Path.Combine(_cacheDirectory, $"{sanitizedKey}.json");
    }

    public async Task SaveAsync<T>(string key, T value)
    {
        var filePath = GetFilePath(key);
        var tempFilePath = $"{filePath}.{Guid.NewGuid():N}.tmp";

        var entry = new CacheEntry<T>(value, DateTime.Now);
        var json = JsonSerializer.Serialize(entry, JsonOptions);

        await FileLock.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(tempFilePath, json);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempFilePath, filePath);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
            FileLock.Release();
        }
    }

    public async Task<CacheEntry<T>?> LoadAsync<T>(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
            return null;

        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<CacheEntry<T>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task ClearAllAsync()
    {
        await FileLock.WaitAsync();
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                var files = Directory.GetFiles(_cacheDirectory, "*.json");
                foreach (var file in files)
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        finally
        {
            FileLock.Release();
        }
    }
}
