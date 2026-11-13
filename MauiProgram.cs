using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Venther.App.Services.Interfaces;
using Venther.App.Services.Mocks;
using Venther.App.ViewModels;
using Venther.App.Views.Alerts;
using Venther.App.Views.Auth;
using Venther.App.Views.Devices;
using Venther.App.Views.Home;
using Venther.App.Views.WorkOrders;

namespace Venther.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter_24pt-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter_24pt-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter_24pt-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter_24pt-Bold.ttf", "InterBold");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
        });
#endif

        // Mock Services (Singleton)
        builder.Services.AddSingleton<IAuthService, MockAuthService>();
        builder.Services.AddSingleton<IDeviceService, MockDeviceService>();
        builder.Services.AddSingleton<IWorkOrderService, MockWorkOrderService>();
        builder.Services.AddSingleton<IAlertService, MockAlertService>();

        // ViewModels & Pages (Transient)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();

        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();

        builder.Services.AddTransient<CheckEmailPage>();
        builder.Services.AddTransient<CheckEmailViewModel>();

        builder.Services.AddTransient<ResetPasswordPage>();
        builder.Services.AddTransient<ResetPasswordViewModel>();

        builder.Services.AddTransient<PasswordResetSuccessPage>();
        builder.Services.AddTransient<PasswordResetSuccessViewModel>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HomeViewModel>();

        builder.Services.AddTransient<AlertsPage>();
        builder.Services.AddTransient<AlertsViewModel>();

        builder.Services.AddTransient<WorkOrdersPage>();
        builder.Services.AddTransient<WorkOrdersViewModel>();

        builder.Services.AddTransient<WorkOrderDetailPage>();
        builder.Services.AddTransient<WorkOrderDetailViewModel>();

        builder.Services.AddTransient<DeviceDetailPage>();
        builder.Services.AddTransient<DeviceDetailViewModel>();

        builder.Services.AddTransient<RegisterDevicePage>();
        builder.Services.AddTransient<RegisterDeviceViewModel>();

        builder.Services.AddTransient<DeviceConnectionPage>();
        builder.Services.AddTransient<DeviceConnectionViewModel>();

        builder.Services.AddTransient<DeviceActivatedPage>();
        builder.Services.AddTransient<DeviceActivatedViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
