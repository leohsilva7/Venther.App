using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;
using Device = Venther.App.Models.Device;

namespace Venther.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private const int PageSize = 20;
    private readonly IDeviceService _deviceService;
    private readonly IAlertService _alertService;
    private readonly ITelemetryHubService _telemetryHub;
    private readonly List<Device> _allDevices = new();
    private readonly HashSet<Guid> _criticalBoardIds = new();
    private CancellationTokenSource? _searchCts;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _isLoadingMore;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _normalCount;

    [ObservableProperty]
    private int _attentionCount;

    [ObservableProperty]
    private string _activeFilter = "Total";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _offlineMessage = string.Empty;

    public ObservableCollection<Device> FilteredDevices { get; } = new();

    public HomeViewModel(IDeviceService deviceService, IAlertService alertService, ITelemetryHubService telemetryHub)
    {
        _deviceService = deviceService;
        _alertService = alertService;
        _telemetryHub = telemetryHub;
        Title = "Início";
        _telemetryHub.TelemetryUpdated += OnTelemetryUpdated;
        _telemetryHub.FaultRaised += OnFaultRaised;
    }

    [RelayCommand]
    public async Task LoadDevicesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            _currentPage = 1;

            var result = await _deviceService.GetDevicesAsync(_currentPage, PageSize);
            _allDevices.Clear();
            _allDevices.AddRange(result.Data.Items);
            _totalPages = Math.Max(1, result.Data.TotalPages);

            // Carregar alertas ativos para marcar equipamentos que já iniciam em falha crítica
            try
            {
                var alertsResult = await _alertService.GetAlertsAsync(1, 100);
                _criticalBoardIds.Clear();
                if (alertsResult.Data?.Items != null)
                {
                    foreach (var alert in alertsResult.Data.Items)
                    {
                        if (!alert.IsResolved && (alert.Severity == AlertSeverity.Critical || alert.Severity == AlertSeverity.Warning))
                        {
                            _criticalBoardIds.Add(alert.BoardId);
                        }
                    }
                }

                foreach (var device in _allDevices)
                {
                    if (Guid.TryParse(device.Id, out var g) && _criticalBoardIds.Contains(g))
                    {
                        device.Status = DeviceStatus.Critical;
                    }
                }
            }
            catch
            {
                // Não bloqueia a listagem caso falhe o carregamento de alertas
            }

            IsOffline = result.FromCache;
            if (result.FromCache && result.CachedAt.HasValue)
            {
                OfflineMessage = $"Sem conexão • dados de {result.CachedAt.Value:dd/MM HH:mm}";
            }
            else
            {
                OfflineMessage = string.Empty;
            }

            UpdateCounts();
            ApplyFilters();
            _ = SubscribeToLoadedBoardsAsync(_allDevices);
        }
        catch (ApiException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Não foi possível carregar os equipamentos.";
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDevicesAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || _isLoadingMore || _currentPage >= _totalPages)
            return;

        try
        {
            _isLoadingMore = true;
            var nextPage = _currentPage + 1;
            var result = await _deviceService.GetDevicesAsync(nextPage, PageSize);

            if (result.Data.Items.Count > 0)
            {
                _currentPage = nextPage;
                _totalPages = Math.Max(1, result.Data.TotalPages);
                foreach (var d in result.Data.Items)
                {
                    if (Guid.TryParse(d.Id, out var g) && _criticalBoardIds.Contains(g))
                    {
                        d.Status = DeviceStatus.Critical;
                    }
                    _allDevices.Add(d);
                }
                UpdateCounts();
                ApplyFilters();
            }
        }
        catch
        {
            // Mantém os itens já carregados caso a paginação falhe
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(300, token).ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully && !token.IsCancellationRequested)
            {
                MainThread.BeginInvokeOnMainThread(ApplyFilters);
            }
        }, TaskScheduler.Default);
    }

    [RelayCommand]
    private void SelectFilter(string filter)
    {
        ActiveFilter = filter;
        ApplyFilters();
    }

    private void UpdateCounts()
    {
        TotalCount = _allDevices.Count;
        NormalCount = _allDevices.Count(d => d.Status == DeviceStatus.Normal);
        AttentionCount = _allDevices.Count(d => d.Status == DeviceStatus.Attention || d.Status == DeviceStatus.Critical);
    }

    private void ApplyFilters()
    {
        var filtered = _allDevices.AsEnumerable();

        if (ActiveFilter == "Normal")
        {
            filtered = filtered.Where(d => d.Status == DeviceStatus.Normal);
        }
        else if (ActiveFilter == "Attention")
        {
            filtered = filtered.Where(d => d.Status == DeviceStatus.Attention || d.Status == DeviceStatus.Critical);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(d =>
                (!string.IsNullOrEmpty(d.Name) && d.Name.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(d.Location) && d.Location.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(d.HardwareId) && d.HardwareId.ToLowerInvariant().Contains(term)));
        }

        var list = filtered.ToList();
        FilteredDevices.Clear();
        foreach (var item in list)
        {
            FilteredDevices.Add(item);
        }
    }

    [RelayCommand]
    private async Task DeviceTapped(Device? device)
    {
        if (device == null)
            return;

        var navParams = new Dictionary<string, object>
        {
            ["DeviceId"] = device.Id
        };

        await Shell.Current.GoToAsync(Routes.DeviceDetail, navParams);
    }

    [RelayCommand]
    private async Task RegisterDevice()
    {
        await Shell.Current.GoToAsync(Routes.RegisterDevice);
    }

    [RelayCommand]
    private async Task OpenAlerts()
    {
        await Shell.Current.GoToAsync("//main/alerts");
    }

    private async Task SubscribeToLoadedBoardsAsync(IEnumerable<Device> devices)
    {
        var ids = devices
            .Select(d => Guid.TryParse(d.Id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        if (ids.Count == 0) return;

        var snapshots = await _telemetryHub.SubscribeAsync(ids);
        foreach (var dto in snapshots)
        {
            ApplyTelemetryToDevice(dto);
        }
    }

    private void OnTelemetryUpdated(BoardTelemetryDto dto)
    {
        ApplyTelemetryToDevice(dto);
    }

    private void OnFaultRaised(FaultRaisedDto fault)
    {
        _criticalBoardIds.Add(fault.BoardId);
        var device = _allDevices.FirstOrDefault(d => Guid.TryParse(d.Id, out var g) && g == fault.BoardId);
        if (device == null) return;

        device.Status = DeviceStatus.Critical;
        device.LastReading = fault.FailureCause;
        UpdateCounts();
        ApplyFilters();
    }

    private void ApplyTelemetryToDevice(BoardTelemetryDto dto)
    {
        var device = _allDevices.FirstOrDefault(d => Guid.TryParse(d.Id, out var g) && g == dto.BoardId);
        if (device == null) return;

        // Se a placa estiver em falha crítica ativa, preserva o status Crítico / Ponto de Atenção
        if (_criticalBoardIds.Contains(dto.BoardId))
        {
            device.Status = DeviceStatus.Critical;
        }
        else
        {
            device.Status = dto.IsOnline ? DeviceStatus.Normal : DeviceStatus.Attention;
        }

        device.LastReading = FormatLastReading(dto.LastSeen);
        UpdateCounts();
        ApplyFilters();
    }

    private static string FormatLastReading(DateTime? lastSeen)
    {
        if (!lastSeen.HasValue)
            return "Sem leitura";

        var diff = DateTime.UtcNow - lastSeen.Value.ToUniversalTime();
        if (diff.TotalMinutes < 1)
            return "Última leitura agora";
        if (diff.TotalMinutes < 60)
            return $"Última leitura há {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Última leitura há {(int)diff.TotalHours}h";
        return $"Última leitura há {(int)diff.TotalDays}d";
    }
}
