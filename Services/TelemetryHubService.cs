using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.ApplicationModel;
using Venther.App.Helpers;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class TelemetryHubService : ITelemetryHubService
{
    private const int MaxTrendPoints = 300;
    private readonly ISessionService _sessionService;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly ConcurrentDictionary<Guid, BoardTelemetryDto> _latestTelemetry = new();
    private readonly ConcurrentDictionary<Guid, List<double>> _trendBuffers = new();
    private readonly HashSet<Guid> _subscribedBoards = new();
    private HubConnection? _connection;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public event Action<BoardTelemetryDto>? TelemetryUpdated;
    public event Action<FaultRaisedDto>? FaultRaised;
    public event Action<bool>? ConnectionStateChanged;

    public TelemetryHubService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task ConnectAsync()
    {
        if (!_sessionService.IsAuthenticated)
            return;

        await _connectLock.WaitAsync();
        try
        {
            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
                return;

            var hubUrl = $"{ApiConfig.BaseUrl.TrimEnd('/')}/hubs/telemetry";
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_sessionService.Token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<BoardTelemetryDto>("TelemetryUpdated", dto =>
            {
                RecordTelemetry(dto);
                MainThread.BeginInvokeOnMainThread(() => TelemetryUpdated?.Invoke(dto));
            });

            _connection.On<FaultRaisedDto>("FaultRaised", dto =>
            {
                MainThread.BeginInvokeOnMainThread(() => FaultRaised?.Invoke(dto));
            });

            _connection.Reconnecting += _ =>
            {
                MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(false));
                return Task.CompletedTask;
            };

            _connection.Closed += _ =>
            {
                MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(false));
                return Task.CompletedTask;
            };

            _connection.Reconnected += async _ =>
            {
                MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(true));
                Guid[] boards;
                lock (_subscribedBoards)
                {
                    boards = _subscribedBoards.ToArray();
                }

                if (boards.Length > 0 && _connection != null)
                {
                    try
                    {
                        await _connection.InvokeAsync<BoardTelemetryDto[]>("SubscribeToBoards", boards);
                    }
                    catch
                    {
                        // Ignora falha na re-assinatura automática
                    }
                }
            };

            await _connection.StartAsync();
            MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(true));
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(false));
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _connectLock.WaitAsync();
        try
        {
            lock (_subscribedBoards)
            {
                _subscribedBoards.Clear();
            }

            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }

            MainThread.BeginInvokeOnMainThread(() => ConnectionStateChanged?.Invoke(false));
        }
        catch
        {
            // Ignora erros ao desconectar
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task<IReadOnlyList<BoardTelemetryDto>> SubscribeAsync(IEnumerable<Guid> boardIds)
    {
        var ids = boardIds.Distinct().ToArray();
        if (ids.Length == 0)
            return Array.Empty<BoardTelemetryDto>();

        lock (_subscribedBoards)
        {
            foreach (var id in ids)
                _subscribedBoards.Add(id);
        }

        if (_connection == null || _connection.State != HubConnectionState.Connected)
            await ConnectAsync();

        if (_connection == null || _connection.State != HubConnectionState.Connected)
            return Array.Empty<BoardTelemetryDto>();

        try
        {
            var result = await _connection.InvokeAsync<BoardTelemetryDto[]>("SubscribeToBoards", ids);
            if (result != null)
            {
                foreach (var dto in result)
                    RecordTelemetry(dto);

                return result;
            }
        }
        catch
        {
            // Retorna vazio se o hub ainda não responder
        }

        return Array.Empty<BoardTelemetryDto>();
    }

    public async Task UnsubscribeAsync(IEnumerable<Guid> boardIds)
    {
        var ids = boardIds.Distinct().ToArray();
        if (ids.Length == 0)
            return;

        lock (_subscribedBoards)
        {
            foreach (var id in ids)
                _subscribedBoards.Remove(id);
        }

        if (_connection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _connection.InvokeAsync("UnsubscribeFromBoards", ids);
            }
            catch
            {
                // Ignora erros ao cancelar assinatura
            }
        }
    }

    public BoardTelemetryDto? GetLatestTelemetry(Guid boardId)
    {
        return _latestTelemetry.TryGetValue(boardId, out var dto) ? dto : null;
    }

    public IReadOnlyList<double> GetTrendBuffer(Guid boardId)
    {
        if (!_trendBuffers.TryGetValue(boardId, out var list))
            return Array.Empty<double>();

        lock (list)
        {
            return list.ToList();
        }
    }

    private void RecordTelemetry(BoardTelemetryDto dto)
    {
        _latestTelemetry[dto.BoardId] = dto;

        var buffer = _trendBuffers.GetOrAdd(dto.BoardId, _ => new List<double>());
        lock (buffer)
        {
            buffer.Add(dto.SupplyAirTemperature);
            if (buffer.Count > MaxTrendPoints)
            {
                buffer.RemoveAt(0);
            }
        }
    }
}
