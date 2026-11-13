using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Venther.App.Helpers;
using Venther.App.Models;
using Device = Venther.App.Models.Device;

namespace Venther.App.Controls;

public partial class DeviceCard : ContentView
{
    public static readonly BindableProperty DeviceProperty =
        BindableProperty.Create(
            nameof(Device),
            typeof(Device),
            typeof(DeviceCard),
            default(Device),
            propertyChanged: (b, _, _) => ((DeviceCard)b).OnDeviceChanged());

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(DeviceCard),
            default(ICommand));

    public Device? Device
    {
        get => (Device?)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public DeviceCard()
    {
        InitializeComponent();
        UpdateStatusBar();
    }

    private void OnDeviceChanged()
    {
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        if (StatusBar == null)
            return;

        var status = Device?.Status ?? DeviceStatus.Normal;
        switch (status)
        {
            case DeviceStatus.Attention:
                StatusBar.Color = GetThemeColor("Accent", Color.FromArgb("#FC6100"));
                break;
            case DeviceStatus.Critical:
                StatusBar.Color = GetThemeColor("Error", Color.FromArgb("#BA1A1A"));
                break;
            case DeviceStatus.Normal:
            default:
                StatusBar.Color = Color.FromArgb("#16A34A");
                break;
        }
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (TapCommand?.CanExecute(Device) == true)
        {
            TapCommand.Execute(Device);
            return;
        }

        if (Shell.Current != null && Device != null)
        {
            var navParams = new Dictionary<string, object>
            {
                ["Device"] = Device,
                ["DeviceId"] = Device.Id
            };
            await Shell.Current.GoToAsync(Routes.DeviceDetail, navParams);
        }
    }

    private Color GetThemeColor(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var val) == true && val is Color color)
        {
            return color;
        }
        return fallback;
    }
}
