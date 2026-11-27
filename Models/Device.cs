using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Venther.App.Models;

public partial class Device : ObservableObject
{
    [ObservableProperty]
    private string id = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private string hardwareId = string.Empty;

    [ObservableProperty]
    private Guid customerId;

    [ObservableProperty]
    private string installationDate = string.Empty;

    [ObservableProperty]
    private DeviceStatus status = DeviceStatus.Normal;

    [ObservableProperty]
    private string lastReading = string.Empty;
}
