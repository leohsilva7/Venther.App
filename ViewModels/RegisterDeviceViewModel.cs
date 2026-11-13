using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Org.Apache.Http.Client.Params;
using System.Collections.ObjectModel;

namespace Venther.App.ViewModels;

public partial class RegisterDeviceViewModel : ObservableObject
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
        FilterClients(string.Empty);
    }

    partial void OnClientNameChanged(string value)
    {
        FilterClients(value);
    }

    private void FilterClients(string query)
    {
        SuggestedClients.Clear();

        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allClients.Take(4)
            : _allClients.Where(c => c.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase));

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
        // Navega para DeviceConnectionPage passando os dados preenchidos
        var navigationParams = new Dictionary<string, object>
        {
            { "ClientName", ClientName.Trim() },
            { "InstallationLocation", InstallationLocation.Trim() }
        };

        await Shell.Current.GoToAsync("DeviceConnectionPage", navigationParams);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}