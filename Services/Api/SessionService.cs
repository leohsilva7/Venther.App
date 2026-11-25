using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Api;

public class SessionService : ISessionService
{
    private const string SessionStorageKey = "venther_user_session";

    private class SessionData
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserDto? User { get; set; }
    }

    private SessionData? _currentSession;

    public bool IsAuthenticated =>
        _currentSession != null &&
        !string.IsNullOrEmpty(_currentSession.Token) &&
        _currentSession.ExpiresAtUtc > DateTime.UtcNow;

    public string? Token => IsAuthenticated ? _currentSession?.Token : null;

    public UserDto? CurrentUser => IsAuthenticated ? _currentSession?.User : null;

    public async Task RestoreAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(SessionStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonSerializer.Deserialize<SessionData>(json);
                if (data != null && data.ExpiresAtUtc > DateTime.UtcNow)
                {
                    _currentSession = data;
                    return;
                }
            }
        }
        catch
        {
            // Fallback caso SecureStorage falhe
        }

        _currentSession = null;
    }

    public async Task SaveAsync(string token, int expiresInSeconds, UserDto user)
    {
        var data = new SessionData
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            User = user
        };

        _currentSession = data;

        try
        {
            var json = JsonSerializer.Serialize(data);
            await SecureStorage.Default.SetAsync(SessionStorageKey, json);
        }
        catch
        {
            // SecureStorage pode falhar em testes ou simuladores sem chaveiro configurado
        }
    }

    public Task ClearAsync()
    {
        _currentSession = null;
        try
        {
            SecureStorage.Default.Remove(SessionStorageKey);
        }
        catch
        {
            // Ignore
        }

        return Task.CompletedTask;
    }
}
