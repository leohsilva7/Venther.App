using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Venther.App.Models;
using Venther.App.Services.Interfaces;
using static Android.Telephony.CarrierConfigManager;

namespace Venther.App.ViewModels;

public record MetricViewData(string Value, string Unit, string Caption, bool IsAlert)
{
    public static MetricViewData Empty => new("--", "", "", false);
}

[QueryProperty(nameof(DeviceId), "id")]
public partial class DeviceDetailViewModel : BaseViewModel
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture; // Figma usa ponto (12.4)
    private readonly IDeviceService _deviceService;

    public DeviceDetailViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        Title = "Detalhes do Equipamento";
    }

    [ObservableProperty] private string? deviceId;
    [ObservableProperty] private bool isRealtimeTab = true;
    [ObservableProperty] private TrendPeriod selectedPeriod = TrendPeriod.OneHour;
    [ObservableProperty] private IReadOnlyList<double> trendValues = Array.Empty<double>();

    [ObservableProperty] private MetricViewData supply = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData returnTemp = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData gas = MetricViewData.Empty;
    [ObservableProperty] private MetricViewData deltaP = MetricViewData.Empty;

    [ObservableProperty] private string statusLabel = "Normal (100%)";
    [ObservableProperty] private bool hasAlert;
    [ObservableProperty] private string nowLabel = "Agora: --";

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
            var id = DeviceId ?? "mock";
            var t = await _deviceService.GetTelemetryAsync(id);

            Supply = new($"{t.Supply.Value.ToString("0.0", Inv)}°C", "", t.Supply.StatusText, t.Supply.IsAlert);
            ReturnTemp = new($"{t.Return.Value.ToString("0.0", Inv)}°C", "", t.Return.StatusText, t.Return.IsAlert);
            Gas = new(t.GasPressure.Value.ToString("0.0", Inv), "bar", t.GasPressure.StatusText, t.GasPressure.IsAlert);
            DeltaP = new(t.EvaporatorDeltaP.Value.ToString("0", Inv), "Pa", t.EvaporatorDeltaP.StatusText, t.EvaporatorDeltaP.IsAlert);

            HasAlert = Supply.IsAlert || ReturnTemp.IsAlert || Gas.IsAlert || DeltaP.IsAlert;
            StatusLabel = $"{(HasAlert ? "Atenção" : "Normal")} ({t.OperationalPercent}%)";
            NowLabel = $"Agora: {t.Supply.Value.ToString("0.0", Inv)}°C";

            TrendValues = await _deviceService.GetTemperatureTrendAsync(id, SelectedPeriod);
        }
        finally { IsBusy = false; }
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
        TrendValues = await _deviceService.GetTemperatureTrendAsync(DeviceId ?? "mock", SelectedPeriod);
    }

    [RelayCommand]
    private Task BackAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private Task RequestMaintenanceAsync() =>
        Shell.Current.DisplayAlert("Manutenção Preventiva", "Solicitação enviada (mock).", "OK"); // TODO: API

    [RelayCommand]
    private Task DownloadReportAsync() =>
        Shell.Current.DisplayAlert("Relatório PMOC", "Download disponível quando a API estiver pronta.", "OK"); // TODO: API
}