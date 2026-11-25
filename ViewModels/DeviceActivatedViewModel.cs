using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(ClientName), "ClientName")]
[QueryProperty(nameof(InstallationLocation), "InstallationLocation")]
[QueryProperty(nameof(HardwareId), "HardwareId")]
public partial class DeviceActivatedViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _clientName = "Hospital Santa Casa";

    [ObservableProperty]
    private string _installationLocation = "Chiller 04 - Cobertura";

    [ObservableProperty]
    private string _hardwareId = "ESP32-NODE-089A";

    [ObservableProperty]
    private string _wifiSsid = "Venther_Tech_2.4G";

    [ObservableProperty]
    private string _signalStrength = "-48 dBm (Excelente)";

    [ObservableProperty]
    private string _initialReading = "22.4°C · Pressão 12.1 bar";

    [ObservableProperty]
    private string _activationTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    public DeviceActivatedViewModel()
    {
        Title = "Dispositivo Ativado";
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
        if (Application.Current?.Windows[0]?.Page != null)
        {
            await Application.Current.Windows[0].Page!.DisplayActionSheetAsync(
                "Opções do Dispositivo",
                "Cancelar",
                null,
                "Ver Detalhes do Dispositivo",
                "Compartilhar Credenciais",
                "Diagnóstico de Rede");
        }
    }
}
