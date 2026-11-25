using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(ClientName), "ClientName")]
[QueryProperty(nameof(InstallationLocation), "InstallationLocation")]
public partial class DeviceConnectionViewModel : BaseViewModel
{
    private readonly IDeviceProvisioningService _provisioningService;

    [ObservableProperty]
    private string _clientName = "Hospital Santa Casa";

    [ObservableProperty]
    private string _installationLocation = "Chiller 04 - Cobertura";

    [ObservableProperty]
    private string _hardwareId = "ESP32-NODE-089A";

    [ObservableProperty]
    private string _macAddress = "24:6F:28:AB:CD:12";

    [ObservableProperty]
    private string _wifiSsid = "Venther_Tech_2.4G";

    [ObservableProperty]
    private string _wifiChannel = "Canal 6 · 2.4 GHz · Sinal Forte (-48 dBm)";

    [ObservableProperty]
    private string _wifiPassword = string.Empty;

    public DeviceConnectionViewModel(IDeviceProvisioningService provisioningService)
    {
        _provisioningService = provisioningService;
        Title = "Conectar Dispositivo";
    }

    [RelayCommand]
    private async Task ProvisionAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            await _provisioningService.ProvisionDeviceAsync(HardwareId, WifiSsid, WifiPassword);

            // Ao concluir, navega para DeviceActivatedPage
            await Shell.Current.GoToAsync(Routes.DeviceActivated, new Dictionary<string, object>
            {
                ["ClientName"] = ClientName,
                ["InstallationLocation"] = InstallationLocation,
                ["HardwareId"] = HardwareId
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
