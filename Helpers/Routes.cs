namespace Venther.App.Helpers;

public static class Routes
{
    // Abas principais / fluxos base
    public const string Login = "login";
    public const string Main = "main";
    public const string Home = "home";
    public const string Alerts = "alerts";
    public const string WorkOrders = "workorders";

    // Telas que abrem por cima das abas
    public const string DeviceDetail = "devicedetail";
    public const string WorkOrderDetail = "workorderdetail";
    public const string RegisterDevice = "registerdevice";
    public const string DeviceConnection = "deviceconnection";
    public const string DeviceActivated = "deviceactivated";

    // Fluxo de autenticação / recuperação de senha
    public const string ForgotPassword = "forgotpassword";
    public const string CheckEmail = "checkemail";
    public const string ResetPassword = "resetpassword";
    public const string PasswordResetSuccess = "passwordresetsuccess";
}
