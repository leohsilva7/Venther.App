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

    protected override bool OnBackButtonPressed()
    {
        // 1. Se houver páginas empilhadas no NavigationStack da aba ativa, desempilha para a página anterior
        if (Current?.Navigation?.NavigationStack.Count > 1)
        {
            Dispatcher.Dispatch(async () => await GoToAsync(".."));
            return true;
        }

        // 2. Se estiver em abas secundárias (Alertas ou Chamados) no nível raiz, o botão voltar redireciona para a Início
        var currentRoute = CurrentState?.Location?.OriginalString ?? string.Empty;
        if (currentRoute.Contains("/alerts") || currentRoute.Contains("/workorders"))
        {
            Dispatcher.Dispatch(async () => await GoToAsync("//main/home"));
            return true;
        }

        // 3. Se estiver na HomePage raiz ou Login, deixa o comportamento padrão do Android (minimizar app)
        return base.OnBackButtonPressed();
    }

    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        // Ao alternar entre abas principais do TabBar (home, alerts, workorders),
        // se houver sub-rotas empilhadas, garantir que a navegação mostre a raiz da aba
        var target = args.Target?.Location?.OriginalString;
        if (!string.IsNullOrEmpty(target) && target.StartsWith("//main/"))
        {
            if (Current?.Navigation?.NavigationStack.Count > 1)
            {
                await Current.Navigation.PopToRootAsync(false);
            }
        }
    }
}
