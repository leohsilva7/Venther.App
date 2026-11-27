using System;
using System.Threading.Tasks;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ISessionService _sessionService;
    private readonly ITelemetryHubService _telemetryHub;
    private readonly ICacheService _cacheService;
    private readonly ISyncService _syncService;

    public bool IsAuthenticated => _sessionService.IsAuthenticated;
    public UserDto? CurrentUser => _sessionService.CurrentUser;

    public AuthService(
        IApiClient apiClient,
        ISessionService sessionService,
        ITelemetryHubService telemetryHub,
        ICacheService cacheService,
        ISyncService syncService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
        _telemetryHub = telemetryHub;
        _cacheService = cacheService;
        _syncService = syncService;
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        var request = new LoginRequestDto(email, password);
        var result = await _apiClient.PostAsync<LoginRequestDto, LoginResponseDto>("/auth/login", request);

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            throw new ApiException("Falha ao processar resposta do servidor.", 500);
        }

        await _sessionService.SaveAsync(result.Token, result.ExpiresIn, result.User);
        _ = _telemetryHub.ConnectAsync();
        return result;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _apiClient.PostAsync<object?>("/auth/logout", null);
        }
        catch
        {
            // Ignora falhas de rede no logout
        }
        finally
        {
            await _telemetryHub.DisconnectAsync();
            await _cacheService.ClearAllAsync();
            await _syncService.ClearQueueAsync();
            await _sessionService.ClearAsync();
        }
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var request = new ForgotPasswordRequestDto(email);
        await _apiClient.PostAsync<ForgotPasswordRequestDto, ForgotPasswordResponseDto>("/auth/forgot-password", request);
    }

    public async Task ResetPasswordAsync(string email, string code, string password, string confirmPassword)
    {
        var request = new ResetPasswordRequestDto(email, code, password, confirmPassword);
        await _apiClient.PostAsync<ResetPasswordRequestDto, ResetPasswordResponseDto>("/auth/reset-password", request);
    }
}
