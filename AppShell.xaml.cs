using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Views.Alerts;
using Venther.App.Views.Auth;
using Venther.App.Views.Devices;
using Venther.App.Views.Home;
using Venther.App.Views.WorkOrders;

namespace Venther.App;

public partial class AppShell : Shell
{
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
    }
}
