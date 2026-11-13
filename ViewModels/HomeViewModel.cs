using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Interfaces;
using Device = Venther.App.Models.Device;

namespace Venther.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IDeviceService _deviceService;
    private List<Device> _allDevices = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _normalCount;

    [ObservableProperty]
    private int _attentionCount;

    [ObservableProperty]
    private string _activeFilter = "Total";

    public ObservableCollection<Device> FilteredDevices { get; } = new();

    public HomeViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        Title = "Início";
        _ = LoadDevicesAsync();
    }

    [RelayCommand]
    public async Task LoadDevicesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var list = await _deviceService.GetDevicesAsync();
            _allDevices = list.ToList();

            UpdateCounts();
            ApplyFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void SelectFilter(string filter)
    {
        ActiveFilter = filter;
        ApplyFilters();
    }

    private void UpdateCounts()
    {
        TotalCount = _allDevices.Count;
        NormalCount = _allDevices.Count(d => d.Status == DeviceStatus.Normal);
        AttentionCount = _allDevices.Count(d => d.Status == DeviceStatus.Attention || d.Status == DeviceStatus.Critical);
    }

    private void ApplyFilters()
    {
        var filtered = _allDevices.AsEnumerable();

        if (ActiveFilter == "Normal")
        {
            filtered = filtered.Where(d => d.Status == DeviceStatus.Normal);
        }
        else if (ActiveFilter == "Attention")
        {
            filtered = filtered.Where(d => d.Status == DeviceStatus.Attention || d.Status == DeviceStatus.Critical);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(d =>
                (!string.IsNullOrEmpty(d.Name) && d.Name.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(d.Location) && d.Location.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(d.Id) && d.Id.ToLowerInvariant().Contains(term)));
        }

        FilteredDevices.Clear();
        foreach (var item in filtered)
        {
            FilteredDevices.Add(item);
        }
    }

    [RelayCommand]
    private async Task DeviceTapped(Device? device)
    {
        if (device == null)
            return;

        var navParams = new Dictionary<string, object>
        {
            ["Device"] = device,
            ["DeviceId"] = device.Id
        };
        await Shell.Current.GoToAsync(Routes.DeviceDetail, navParams);
    }

    [RelayCommand]
    private async Task RegisterDevice()
    {
        await Shell.Current.GoToAsync(Routes.RegisterDevice);
    }

    [RelayCommand]
    private async Task OpenAlerts()
    {
        await Shell.Current.GoToAsync("//main/alerts");
    }
}
