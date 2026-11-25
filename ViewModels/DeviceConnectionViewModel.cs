using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(CustomerId), "CustomerId")]
[QueryProperty(nameof(ClientName), "ClientName")]
[QueryProperty(nameof(InstallationLocation), "InstallationLocation")]
public partial class DeviceConnectionViewModel : BaseViewModel
{
    private static readonly Regex MacRegex = new(@"^([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled);

    private readonly IDeviceProvisioningService _provisioningService;
    private readonly IDeviceService _deviceService;

    [ObservableProperty]
    private string _customerId = string.Empty;

    [ObservableProperty]
    private string _clientName = string.Empty;

    [ObservableProperty]
    private string _installationLocation = string.Empty;

    [ObservableProperty]
    private string _hardwareId = "24:6F:28:AB:CD:12";

    [ObservableProperty]
    private string _macAddress = "24:6F:28:AB:CD:12";

    [ObservableProperty]
    private string _wifiSsid = "Venther_Tech_2.4G";

    [ObservableProperty]
    private string _wifiChannel = "Canal 6 · 2.4 GHz · Sinal Forte (-48 dBm)";

    [ObservableProperty]
    private string _wifiPassword = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public DeviceConnectionViewModel(IDeviceProvisioningService provisioningService, IDeviceService deviceService)
    {
        _provisioningService = provisioningService;
        _deviceService = deviceService;
        Title = "Conectar Dispositivo";
    }

    partial void OnMacAddressChanged(string value)
    {
        HardwareId = value?.Trim() ?? string.Empty;
        if (HasError)
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task ProvisionAsync()
    {
        if (IsBusy)
            return;

        HasError = false;
        ErrorMessage = string.Empty;

        var mac = MacAddress?.Trim() ?? string.Empty;
        if (!MacRegex.IsMatch(mac))
        {
            HasError = true;
            ErrorMessage = "Informe um endereço MAC válido (ex.: AA:BB:CC:DD:EE:FF).";
            return;
        }

        if (!Guid.TryParse(CustomerId, out var customerGuid))
        {
            HasError = true;
            ErrorMessage = "Cliente inválido. Volte e selecione um cliente.";
            return;
        }

        IsBusy = true;
        try
        {
            var provisioned = await _provisioningService.ProvisionDeviceAsync(mac, WifiSsid, WifiPassword);
            if (!provisioned)
            {
                HasError = true;
                ErrorMessage = "Falha no provisionamento Wi-Fi do dispositivo.";
                return;
            }

            var created = await _deviceService.RegisterDeviceAsync(customerGuid, mac, InstallationLocation);

            await Shell.Current.GoToAsync(Routes.DeviceActivated, new Dictionary<string, object>
            {
                ["ClientName"] = ClientName,
                ["InstallationLocation"] = InstallationLocation,
                ["HardwareId"] = created.HardwareId,
                ["WifiSsid"] = WifiSsid,
                ["ActivationTimestamp"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });
        }
        catch (ApiException ex)
        {
            HasError = true;
            ErrorMessage = ex.StatusCode == 409
                ? "Este MAC já está cadastrado."
                : ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Não foi possível cadastrar o dispositivo. Tente novamente.";
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
