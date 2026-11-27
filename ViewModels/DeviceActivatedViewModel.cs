using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(BoardId), "BoardId")]
[QueryProperty(nameof(ClientName), "ClientName")]
[QueryProperty(nameof(InstallationLocation), "InstallationLocation")]
[QueryProperty(nameof(HardwareId), "HardwareId")]
[QueryProperty(nameof(WifiSsid), "WifiSsid")]
[QueryProperty(nameof(ActivationTimestamp), "ActivationTimestamp")]
public partial class DeviceActivatedViewModel : BaseViewModel
{
    private readonly ITelemetryHubService _telemetryHub;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _boardId = string.Empty;

    [ObservableProperty]
    private string _clientName = string.Empty;

    [ObservableProperty]
    private string _installationLocation = string.Empty;

    [ObservableProperty]
    private string _hardwareId = string.Empty;

    [ObservableProperty]
    private string _wifiSsid = string.Empty;

    [ObservableProperty]
    private string _signalStrength = "Conectado";

    [ObservableProperty]
    private string _initialReading = "Aguardando primeira leitura";

    [ObservableProperty]
    private string _activationTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    public DeviceActivatedViewModel(ITelemetryHubService telemetryHub, IDialogService dialogService)
    {
        _telemetryHub = telemetryHub;
        _dialogService = dialogService;
        Title = "Dispositivo Ativado";
    }

    partial void OnBoardIdChanged(string value)
    {
        if (Guid.TryParse(value, out var guid))
        {
            var telemetry = _telemetryHub.GetLatestTelemetry(guid);
            if (telemetry != null)
            {
                InitialReading = $"{telemetry.SupplyAirTemperature.ToString("0.0", CultureInfo.InvariantCulture)}°C · ΔP {telemetry.EvaporatorCoilPressureDrop.ToString("0", CultureInfo.InvariantCulture)} Pa";
            }
        }
    }

    [RelayCommand]
    private async Task GoHomeAsync()
    {
        await Shell.Current.GoToAsync("//main/home");
    }

    [RelayCommand]
    private async Task RegisterAnotherDeviceAsync()
    {
        try
        {
            await Shell.Current.GoToAsync($"//main/home/{Routes.RegisterDevice}");
        }
        catch
        {
            await Shell.Current.GoToAsync(Routes.RegisterDevice);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task MoreOptionsAsync()
    {
        var selected = await _dialogService.ShowActionSheetAsync(
            "Opções do Dispositivo",
            "Cancelar",
            "Ver Detalhes do Dispositivo",
            "Compartilhar Credenciais",
            "Diagnóstico de Rede");

        if (selected == "Ver Detalhes do Dispositivo" && !string.IsNullOrWhiteSpace(BoardId))
        {
            await Shell.Current.GoToAsync(Routes.DeviceDetail, new System.Collections.Generic.Dictionary<string, object>
            {
                ["DeviceId"] = BoardId
            });
        }
    }
}
