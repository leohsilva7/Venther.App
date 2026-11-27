using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;
using Venther.App.Views.Alerts;
using Venther.App.Views.Auth;
using Venther.App.Views.Devices;
using Venther.App.Views.Home;
using Venther.App.Views.WorkOrders;

namespace Venther.App;

public partial class AppShell : Shell
{
    private readonly ISessionService? _sessionService;

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Routes.DeviceDetail, typeof(DeviceDetailPage));
        Routing.RegisterRoute(Routes.WorkOrderDetail, typeof(WorkOrderDetailPage));
        Routing.RegisterRoute(Routes.RegisterDevice, typeof(RegisterDevicePage));
        Routing.RegisterRoute(Routes.DeviceConnection, typeof(DeviceConnectionPage));
        Routing.RegisterRoute(Routes.DeviceActivated, typeof(DeviceActivatedPage));

        Routing.RegisterRoute(Routes.ForgotPassword, typeof(ForgotPasswordPage));
        Routing.RegisterRoute(Routes.CheckEmail, typeof(CheckEmailPage));
        Routing.RegisterRoute(Routes.ResetPassword, typeof(ResetPasswordPage));
        Routing.RegisterRoute(Routes.PasswordResetSuccess, typeof(PasswordResetSuccessPage));

        _sessionService = IPlatformApplication.Current?.Services.GetService(typeof(ISessionService)) as ISessionService;

        if (IPlatformApplication.Current?.Services.GetService(typeof(IAppNotificationService)) is IAppNotificationService notificationService)
        {
            notificationService.Initialize();

            if (IPlatformApplication.Current?.Services.GetService(typeof(ITelemetryHubService)) is ITelemetryHubService hub)
            {
                hub.FaultRaised += async (fault) =>
                {
                    await notificationService.NotifyFaultAsync(fault.BoardId, fault.FailureCause, fault.Date);
                };
            }
        }
    }

    protected override async void OnParentSet()
    {
        base.OnParentSet();

        if (Parent != null && _sessionService != null)
        {
            await _sessionService.RestoreAsync();

            if (_sessionService.IsAuthenticated)
            {
                if (IPlatformApplication.Current?.Services.GetService(typeof(IAppNotificationService)) is IAppNotificationService notificationService)
                {
                    _ = notificationService.RequestPermissionAsync();
                }
                if (IPlatformApplication.Current?.Services.GetService(typeof(ITelemetryHubService)) is ITelemetryHubService hub)
                {
                    _ = hub.ConnectAsync();
                }
                if (IPlatformApplication.Current?.Services.GetService(typeof(ISyncService)) is ISyncService sync)
                {
                    _ = sync.ProcessQueueAsync();
                }
                await GoToAsync("//main/home");
            }
            else
            {
                await GoToAsync("//login");
            }
        }
    }
}
