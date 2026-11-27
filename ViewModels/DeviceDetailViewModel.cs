using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DeviceModel = Venther.App.Models.Device;

namespace Venther.App.ViewModels;

public record MetricViewData(string Value, string Unit, string Caption, bool IsAlert)
{
    public static MetricViewData Empty => new("--", "", "Sem leitura", false);
}

public record HistoryLogItem(string Timestamp, string Supply, string Return, string GasLine, string DeltaP);

[QueryProperty(nameof(DeviceId), "DeviceId")]
public partial class DeviceDetailViewModel : BaseViewModel, IQueryAttributable
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly IDeviceService _deviceService;
    private readonly IAlertService _alertService;
    private readonly ITelemetryHubService _telemetryHub;
    private readonly IDialogService _dialogService;
    private Guid _boardGuid;

    public DeviceDetailViewModel(IDeviceService deviceService, IAlertService alertService, ITelemetryHubService telemetryHub, IDialogService dialogService)
    {
        _deviceService = deviceService;
        _alertService = alertService;
        _telemetryHub = telemetryHub;
        _dialogService = dialogService;
        Title = "Detalhes do Equipamento";
        _telemetryHub.TelemetryUpdated += OnTelemetryUpdated;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("DeviceId", out var idVal) && idVal != null)
        {
            DeviceId = idVal.ToString();
        }
        else if (query.TryGetValue("BoardId", out var boardIdVal) && boardIdVal != null)
        {
            DeviceId = boardIdVal.ToString();
        }
        else if (query.TryGetValue("Id", out var rawId) && rawId != null)
        {
            DeviceId = rawId.ToString();
        }
        else if (query.TryGetValue("Alert", out var alertObj) && alertObj is AlertItem alertItem)
        {
            if (alertItem.BoardId != Guid.Empty)
            {
                DeviceId = alertItem.BoardId.ToString();
            }
        }
        else if (query.TryGetValue("Device", out var devObj) && devObj is DeviceModel dev)
        {
            if (!string.IsNullOrEmpty(dev.Id))
            {
                DeviceId = dev.Id;
            }
        }

        if (Guid.TryParse(DeviceId, out _boardGuid) && _boardGuid != Guid.Empty)
        {
            _ = LoadAsync();
        }
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

    // History tab
    private static readonly CultureInfo PtBr = new("pt-BR");
    private DateTime _selectedMonth = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    private List<HistoryLogItem> _allHistoryLogs = new();
    private bool _historyLoaded;

    [ObservableProperty] private string selectedMonthLabel = string.Empty;
    [ObservableProperty] private bool canGoNextMonth;
    [ObservableProperty] private bool isHistoryLoading;
    [ObservableProperty] private bool hasHistoryError;
    [ObservableProperty] private string historyErrorMessage = string.Empty;
    [ObservableProperty] private bool isHistoryEmpty;
    [ObservableProperty] private string historyEmptyMessage = "Sem leituras neste mês";
    [ObservableProperty] private bool hasHistoryData;
    [ObservableProperty] private string supplySummary = "--";
    [ObservableProperty] private string returnSummary = "--";
    [ObservableProperty] private string totalReadingsCount = "0";
    [ObservableProperty] private bool hasMoreHistoryLogs;
    public ObservableCollection<HistoryLogItem> VisibleHistoryLogs { get; } = new();

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

            // Carrega os dados cadastrais da placa (nome, localização, etc.)
            try
            {
                var deviceResult = await _deviceService.GetDeviceByIdAsync(DeviceId);
                if (deviceResult.Data != null)
                {
                    Title = string.IsNullOrWhiteSpace(deviceResult.Data.Name)
                        ? $"Placa {deviceResult.Data.HardwareId}"
                        : deviceResult.Data.Name;
                }
            }
            catch
            {
                // Fallback para manter a página utilizável se falhar a busca cadastral
            }

            // Verifica se há falhas/alertas ativos para refletir no badge de status
            bool hasActiveFailure = false;
            try
            {
                var alertsRes = await _alertService.GetAlertsAsync(1, 100);
                if (alertsRes.Data?.Items != null)
                {
                    hasActiveFailure = alertsRes.Data.Items.Any(a => !a.IsResolved && a.BoardId == _boardGuid);
                }
            }
            catch
            {
                // Ignora se não conseguir carregar alertas
            }

            var initial = await _telemetryHub.SubscribeAsync(new[] { _boardGuid });
            var latest = initial.FirstOrDefault() ?? _telemetryHub.GetLatestTelemetry(_boardGuid);

            if (latest != null)
            {
                ApplyTelemetry(latest, hasActiveFailure);
            }
            else
            {
                // Fallback: busca último registro em /logs antes de cair em ApplyNoData
                var loadedFromLog = false;
                try
                {
                    var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
                    var logs = await _deviceService.GetBoardLogsAsync(_boardGuid, currentMonth);
                    var lastLog = logs.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
                    if (lastLog != null)
                    {
                        var dtoFromLog = new BoardTelemetryDto(
                            BoardId: _boardGuid,
                            MacAddress: string.Empty,
                            SupplyAirTemperature: lastLog.SupplyAirTemperature,
                            ReturnAirTemperature: lastLog.ReturnAirTemperature,
                            CondensationAirTemperature: lastLog.CondensationAirTemperature,
                            SpaceTemperature: lastLog.SpaceTemperature,
                            GasLineTemperature: lastLog.GasLineTemperature,
                            LiquidLineTemperature: lastLog.LiquidLineTemperature,
                            EvaporatorCoilPressureDrop: lastLog.EvaporatorCoilPressureDrop,
                            CondenserCoilPressureDrop: lastLog.CondenserCoilPressureDrop,
                            IsOnline: lastLog.CreatedAt.HasValue && (DateTime.UtcNow - lastLog.CreatedAt.Value.ToUniversalTime()).TotalMinutes < 5,
                            LastSeen: lastLog.CreatedAt
                        );
                        ApplyTelemetry(dtoFromLog, hasActiveFailure);
                        loadedFromLog = true;
                    }
                }
                catch
                {
                    // Fallback falhou
                }

                if (!loadedFromLog)
                {
                    ApplyNoData();
                    if (hasActiveFailure)
                    {
                        HasAlert = true;
                        StatusLabel = "Crítico";
                    }
                }
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

    private void ApplyTelemetry(BoardTelemetryDto t, bool forceAlert = false)
    {
        var isAlert = forceAlert || !t.IsOnline;
        var caption = t.IsOnline ? "Ao vivo" : "Offline";

        Supply = new($"{t.SupplyAirTemperature.ToString("0.0", Inv)}°C", "", caption, isAlert);
        ReturnTemp = new($"{t.ReturnAirTemperature.ToString("0.0", Inv)}°C", "", caption, isAlert);
        // Se o valor for maior que 35, representa a pressão em PSI do sensor de sucção R410A; caso contrário, temperatura em °C
        var gasUnit = t.GasLineTemperature > 35.0 ? "PSI" : "°C";
        Gas = new($"{t.GasLineTemperature.ToString("0.0", Inv)} {gasUnit}", "", caption, isAlert);
        DeltaP = new(t.EvaporatorCoilPressureDrop.ToString("0", Inv), "Pa", caption, isAlert);

        HasAlert = isAlert;
        StatusLabel = forceAlert ? "Crítico" : (t.IsOnline ? "Online" : "Offline");
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
    private async Task SelectTabAsync(string tab)
    {
        IsRealtimeTab = tab == "realtime";
        if (IsHistoryTab && !_historyLoaded)
        {
            UpdateMonthHeader();
            await LoadHistoryAsync();
        }
    }

    private void UpdateMonthHeader()
    {
        var raw = _selectedMonth.ToString("MMMM yyyy", PtBr);
        SelectedMonthLabel = char.ToUpper(raw[0], PtBr) + raw[1..];
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        CanGoNextMonth = _selectedMonth < currentMonth;
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        _selectedMonth = _selectedMonth.AddMonths(-1);
        UpdateMonthHeader();
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        if (!CanGoNextMonth) return;
        _selectedMonth = _selectedMonth.AddMonths(1);
        UpdateMonthHeader();
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        if (IsHistoryLoading || _boardGuid == Guid.Empty) return;

        IsHistoryLoading = true;
        HasHistoryError = false;
        HistoryErrorMessage = string.Empty;
        IsHistoryEmpty = false;
        HasHistoryData = false;
        HistoryEmptyMessage = "Sem leituras neste mês";
        VisibleHistoryLogs.Clear();
        HasMoreHistoryLogs = false;

        try
        {
            var logs = await _deviceService.GetBoardLogsAsync(_boardGuid, _selectedMonth.ToString("yyyy-MM"));
            _historyLoaded = true;

            if (logs.Count == 0)
            {
                IsHistoryEmpty = true;
                return;
            }

            var ordered = logs.OrderByDescending(l => l.CreatedAt ?? DateTime.MinValue).ToList();

            var sMin = ordered.Min(l => l.SupplyAirTemperature).ToString("0.0", Inv);
            var sAvg = ordered.Average(l => l.SupplyAirTemperature).ToString("0.0", Inv);
            var sMax = ordered.Max(l => l.SupplyAirTemperature).ToString("0.0", Inv);
            SupplySummary = $"{sMin}° / {sAvg}° / {sMax}°C";

            var rMin = ordered.Min(l => l.ReturnAirTemperature).ToString("0.0", Inv);
            var rAvg = ordered.Average(l => l.ReturnAirTemperature).ToString("0.0", Inv);
            var rMax = ordered.Max(l => l.ReturnAirTemperature).ToString("0.0", Inv);
            ReturnSummary = $"{rMin}° / {rAvg}° / {rMax}°C";

            TotalReadingsCount = ordered.Count.ToString(Inv);

            _allHistoryLogs = ordered.Select(l => new HistoryLogItem(
                Timestamp: l.CreatedAt.HasValue ? l.CreatedAt.Value.ToLocalTime().ToString("dd/MM HH:mm", PtBr) : "--",
                Supply: $"{l.SupplyAirTemperature.ToString("0.0", Inv)}°C",
                Return: $"{l.ReturnAirTemperature.ToString("0.0", Inv)}°C",
                GasLine: $"{l.GasLineTemperature.ToString("0.0", Inv)}°C",
                DeltaP: $"{l.EvaporatorCoilPressureDrop.ToString("0", Inv)} Pa"
            )).ToList();

            foreach (var item in _allHistoryLogs.Take(30))
                VisibleHistoryLogs.Add(item);

            HasMoreHistoryLogs = _allHistoryLogs.Count > VisibleHistoryLogs.Count;
            HasHistoryData = true;
        }
        catch (ApiException ex) when (ex.StatusCode is 401 or 403 or 404)
        {
            IsHistoryEmpty = true;
            HistoryEmptyMessage = "Histórico indisponível para este perfil";
        }
        catch (ApiException ex)
        {
            HasHistoryError = true;
            HistoryErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasHistoryError = true;
            HistoryErrorMessage = "Não foi possível carregar o histórico.";
        }
        finally
        {
            IsHistoryLoading = false;
        }
    }

    [RelayCommand]
    private void ShowMoreHistory()
    {
        var next = _allHistoryLogs.Skip(VisibleHistoryLogs.Count).Take(30);
        foreach (var item in next)
            VisibleHistoryLogs.Add(item);

        HasMoreHistoryLogs = _allHistoryLogs.Count > VisibleHistoryLogs.Count;
    }

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
        _dialogService.ShowAlertAsync("Manutenção Preventiva", "Funcionalidade de solicitação de manutenção preventiva em breve.", "Entendi", DialogType.Info);

    [RelayCommand]
    private Task DownloadReportAsync() =>
        _dialogService.ShowAlertAsync("Relatório PMOC", "O download do relatório PMOC estará disponível em breve.", "Entendi", DialogType.Info);
}
