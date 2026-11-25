using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;

namespace Venther.App.ViewModels;

public partial class RegisterDeviceViewModel : BaseViewModel
{
    private readonly List<string> _allClients =
    [
        "Hospital Santa Casa",
        "Supermercado Bom Preço",
        "Frigorífico Sul",
        "Farmácia Central",
        "Centro Logístico Norte",
        "Indústria Laticínios Vale"
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyCanExecuteChangedFor(nameof(AdvanceCommand))]
    private string _clientName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyCanExecuteChangedFor(nameof(AdvanceCommand))]
    private string _installationLocation = string.Empty;

    public ObservableCollection<string> SuggestedClients { get; } = [];

    public bool CanProceed =>
        !string.IsNullOrWhiteSpace(ClientName) &&
        !string.IsNullOrWhiteSpace(InstallationLocation);

    public RegisterDeviceViewModel()
    {
        Title = "Cadastrar Dispositivo";
        FilterClients(string.Empty);
    }

    partial void OnClientNameChanged(string value)
    {
        FilterClients(value);
    }

    private void FilterClients(string query)
    {
        SuggestedClients.Clear();

        var trimmed = query?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(trimmed)
            ? _allClients.Take(4)
            : _allClients.Where(c => c.Contains(trimmed, StringComparison.OrdinalIgnoreCase));

        foreach (var client in filtered)
        {
            SuggestedClients.Add(client);
        }
    }

    [RelayCommand]
    private void SelectClient(string? client)
    {
        if (!string.IsNullOrWhiteSpace(client))
        {
            ClientName = client;
        }
    }

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private async Task AdvanceAsync()
    {
        if (!CanProceed)
            return;

        var navigationParams = new Dictionary<string, object>
        {
            ["ClientName"] = ClientName.Trim(),
            ["InstallationLocation"] = InstallationLocation.Trim()
        };

        await Shell.Current.GoToAsync(Routes.DeviceConnection, navigationParams);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
