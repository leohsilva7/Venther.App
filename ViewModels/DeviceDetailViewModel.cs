using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public record MetricViewData(string Value, string Unit, string Caption, bool IsAlert)
{
    public static MetricViewData Empty => new("--", "", "Sem leitura", false);
}

[QueryProperty(nameof(DeviceId), "DeviceId")]
public partial class DeviceDetailViewModel : BaseViewModel
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly IDeviceService _deviceService;
    private readonly ITelemetryHubService _telemetryHub;
    private Guid _boardGuid;

    public DeviceDetailViewModel(IDeviceService deviceService, ITelemetryHubService telemetryHub)
    {
        _deviceService = deviceService;
        _telemetryHub = telemetryHub;
        Title = "Detalhes do Equipamento";
        _telemetryHub.TelemetryUpdated += OnTelemetryUpdated;
    }

    [ObservableProperty] private string? deviceId;
    [ObservableProperty] private bool isRealtimeTab = true;
    [ObservableProperty] private TrendPeriod selectedPeriod = TrendPeriod.OneHour;
    [ObservableProperty] private IReadOnlyList<double> trendValues = Array.Empty<double>();

    [ObservableProperty] private MetricViewData supply = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData returnTemp = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData gas = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData deltaP = MetricViewData.Empty;

    [ObservableProperty] private string statusLabel = "Offline";
    [ObservableProperty] private bool hasAlert;
    [ObservableProperty] private string nowLabel = "Sem leitura";
    [ObservableProperty] private string historyStatusText = string.Empty;

    public bool IsHistoryTab => !IsRealtimeTab;
    public bool IsPeriod1h => SelectedPeriod == TrendPeriod.OneHour;
    public bool IsPeriod12h => SelectedPeriod == TrendPeriod.TwelveHours;
    public bool IsPeriod24h => SelectedPeriod == TrendPeriod.TwentyFourHours;
    public string TrendStartLabel => SelectedPeriod switch
    {
        TrendPeriod.OneHour => "Há 60 minutos",
        TrendPeriod.TwelveHours => "Há 12 horas",
        _ => "Há 24 horas"
    };

    partial void OnIsRealtimeTabChanged(bool value) => OnPropertyChanged(nameof(IsHistoryTab));

    partial void OnSelectedPeriodChanged(TrendPeriod value)
    {
        OnPropertyChanged(nameof(IsPeriod1h));
        OnPropertyChanged(nameof(IsPeriod12h));
        OnPropertyChanged(nameof(IsPeriod24h));
        OnPropertyChanged(nameof(TrendStartLabel));
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            if (!Guid.TryParse(DeviceId, out _boardGuid))
            {
                ApplyNoData();
                return;
            }

            var initial = await _telemetryHub.SubscribeAsync(new[] { _boardGuid });
            var latest = initial.FirstOrDefault() ?? _telemetryHub.GetLatestTelemetry(_boardGuid);

            if (latest != null)
            {
                ApplyTelemetry(latest);
            }
            else
            {
                ApplyNoData();
            }

            await LoadTrendAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CleanupAsync()
    {
        _telemetryHub.TelemetryUpdated -= OnTelemetryUpdated;
        if (_boardGuid != Guid.Empty)
        {
            await _telemetryHub.UnsubscribeAsync(new[] { _boardGuid });
        }
    }

    private void OnTelemetryUpdated(BoardTelemetryDto dto)
    {
        if (_boardGuid == Guid.Empty || dto.BoardId != _boardGuid)
            return;

        ApplyTelemetry(dto);
        if (SelectedPeriod == TrendPeriod.OneHour)
        {
            TrendValues = _telemetryHub.GetTrendBuffer(_boardGuid);
        }
    }

    private void ApplyNoData()
    {
        Supply = MetricViewData.Empty;
        ReturnTemp = MetricViewData.Empty;
        Gas = MetricViewData.Empty;
        DeltaP = MetricViewData.Empty;
        HasAlert = true;
        StatusLabel = "Offline";
        NowLabel = "Sem leitura";
    }

    private void ApplyTelemetry(BoardTelemetryDto t)
    {
        var caption = t.IsOnline ? "Ao vivo" : "Offline";
        var isAlert = !t.IsOnline;

        Supply = new($"{t.SupplyAirTemperature.ToString("0.0", Inv)}°C", "", caption, isAlert);
        ReturnTemp = new($"{t.ReturnAirTemperature.ToString("0.0", Inv)}°C", "", caption, isAlert);
        Gas = new($"{t.GasLineTemperature.ToString("0.0", Inv)}°C", "", caption, isAlert);
        DeltaP = new(t.EvaporatorCoilPressureDrop.ToString("0", Inv), "Pa", caption, isAlert);

        HasAlert = isAlert;
        StatusLabel = t.IsOnline ? "Online" : "Offline";
        NowLabel = FormatLastSeen(t.LastSeen);
    }

    private static string FormatLastSeen(DateTime? lastSeen)
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

    private async Task LoadTrendAsync()
    {
        HistoryStatusText = string.Empty;
        if (_boardGuid == Guid.Empty)
        {
            TrendValues = Array.Empty<double>();
            return;
        }

        if (SelectedPeriod == TrendPeriod.OneHour)
        {
            TrendValues = _telemetryHub.GetTrendBuffer(_boardGuid);
            return;
        }

        var hours = SelectedPeriod == TrendPeriod.TwelveHours ? 12 : 24;
        var cutoff = DateTime.UtcNow.AddHours(-hours);

        try
        {
            var now = DateTime.UtcNow;
            var logs = (await _deviceService.GetBoardLogsAsync(_boardGuid, now.ToString("yyyy-MM"))).ToList();

            if (cutoff.Month != now.Month)
            {
                var prevLogs = await _deviceService.GetBoardLogsAsync(_boardGuid, cutoff.ToString("yyyy-MM"));
                logs.AddRange(prevLogs);
            }

            var points = logs
                .Where(l => l.CreatedAt.HasValue && l.CreatedAt.Value.ToUniversalTime() >= cutoff)
                .OrderBy(l => l.CreatedAt)
                .Select(l => l.SupplyAirTemperature)
                .ToList();

            TrendValues = Downsample(points, 60);
        }
        catch (ApiException ex) when (ex.StatusCode is 401 or 403 or 404)
        {
            HistoryStatusText = "Histórico indisponível";
            TrendValues = _telemetryHub.GetTrendBuffer(_boardGuid);
        }
        catch
        {
            HistoryStatusText = "Histórico indisponível";
            TrendValues = _telemetryHub.GetTrendBuffer(_boardGuid);
        }
    }

    private static IReadOnlyList<double> Downsample(IReadOnlyList<double> source, int maxPoints)
    {
        if (source.Count <= maxPoints)
            return source;

        var result = new List<double>(maxPoints);
        var step = (double)(source.Count - 1) / (maxPoints - 1);
        for (int i = 0; i < maxPoints; i++)
        {
            result.Add(source[(int)Math.Round(i * step)]);
        }
        return result;
    }

    [RelayCommand]
    private void SelectTab(string tab) => IsRealtimeTab = tab == "realtime";

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period switch
        {
            "12h" => TrendPeriod.TwelveHours,
            "24h" => TrendPeriod.TwentyFourHours,
            _ => TrendPeriod.OneHour
        };
        await LoadTrendAsync();
    }

    [RelayCommand]
    private Task BackAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private Task RequestMaintenanceAsync() =>
        Shell.Current.DisplayAlertAsync("Manutenção Preventiva", "Em breve.", "OK");

    [RelayCommand]
    private Task DownloadReportAsync() =>
        Shell.Current.DisplayAlertAsync("Relatório PMOC", "Em breve.", "OK");
}
