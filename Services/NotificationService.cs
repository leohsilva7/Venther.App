using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using Plugin.LocalNotification.Core.Models;
using Venther.App.Helpers;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public interface IAppNotificationService
{
    Task RequestPermissionAsync();
    void Initialize();
    Task NotifyFaultAsync(Guid boardId, string failureCause, string date);
}

public class AppNotificationService : IAppNotificationService
{
    private readonly IDeviceService _deviceService;
    private readonly ConcurrentDictionary<string, bool> _notifiedFaults = new();
    private bool _initialized;
    private int _notificationIdCounter = 1000;

    public AppNotificationService(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
    }

    private void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (e.IsDismissed) return;

        var returningData = e.Request?.ReturningData;
        if (!string.IsNullOrWhiteSpace(returningData))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.GoToAsync(Routes.DeviceDetail, new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["DeviceId"] = returningData
                    });
                }
                catch
                {
                    // Ignora falha de navegação se o Shell ainda não estiver pronto
                }
            });
        }
    }

    public async Task RequestPermissionAsync()
    {
        try
        {
            var isGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!isGranted)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
        }
        catch
        {
            // Ignora se a plataforma não suportar requisição direta
        }
    }

    public async Task NotifyFaultAsync(Guid boardId, string failureCause, string date)
    {
        var deduplicationKey = $"{boardId:D}_{date}";
        if (!_notifiedFaults.TryAdd(deduplicationKey, true))
            return;

        string deviceName = boardId.ToString();
        try
        {
            var deviceResult = await _deviceService.GetDeviceByIdAsync(boardId.ToString());
            if (deviceResult?.Data != null && !string.IsNullOrWhiteSpace(deviceResult.Data.Name))
            {
                deviceName = deviceResult.Data.Name;
            }
        }
        catch
        {
            // Fallback para o boardId
        }

        var notificationId = System.Threading.Interlocked.Increment(ref _notificationIdCounter);

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = $"Falha em {deviceName}",
            Description = failureCause,
            ReturningData = boardId.ToString()
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}
