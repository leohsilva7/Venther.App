using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class RegisterDeviceViewModel : BaseViewModel
{
    private readonly IClientService _clientService;
    private List<CustomerDto> _allClients = new();
    private Guid? _selectedCustomerId;
    private string _selectedClientName = string.Empty;

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
        _selectedCustomerId.HasValue &&
        !string.IsNullOrWhiteSpace(ClientName) &&
        !string.IsNullOrWhiteSpace(InstallationLocation);

    public RegisterDeviceViewModel(IClientService clientService)
    {
        _clientService = clientService;
        Title = "Cadastrar Dispositivo";
        _ = LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            var result = await _clientService.GetCustomersAsync(1, 100);
            _allClients = result.Items.ToList();
            FilterClients(ClientName);
        }
        catch
        {
            // Mantém lista vazia em caso de erro de rede
        }
    }

    partial void OnClientNameChanged(string value)
    {
        if (!string.Equals(value?.Trim(), _selectedClientName, StringComparison.OrdinalIgnoreCase))
        {
            _selectedCustomerId = null;
        }
        FilterClients(value ?? string.Empty);
    }

    private void FilterClients(string query)
    {
        SuggestedClients.Clear();

        var trimmed = query?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(trimmed)
            ? _allClients.Take(3)
            : _allClients.Where(c => c.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)).Take(3);

        foreach (var client in filtered)
        {
            SuggestedClients.Add(client.Name);
        }
    }

    [RelayCommand]
    private void SelectClient(string? client)
    {
        if (string.IsNullOrWhiteSpace(client))
            return;

        var match = _allClients.FirstOrDefault(c => string.Equals(c.Name, client, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            _selectedCustomerId = match.Id;
            _selectedClientName = match.Name;
            ClientName = match.Name;
        }
    }

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private async Task AdvanceAsync()
    {
        if (!CanProceed || !_selectedCustomerId.HasValue)
            return;

        var navigationParams = new Dictionary<string, object>
        {
            ["CustomerId"] = _selectedCustomerId.Value.ToString(),
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
