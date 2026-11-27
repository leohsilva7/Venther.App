using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _hasEmailError;

    [ObservableProperty]
    private string _emailErrorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasPasswordError;

    [ObservableProperty]
    private string _passwordErrorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasCredentialError;

    [ObservableProperty]
    private string _credentialErrorMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Login";
    }

    partial void OnEmailChanged(string value)
    {
        if (HasEmailError)
        {
            HasEmailError = false;
            EmailErrorMessage = string.Empty;
            if (!HasPasswordError)
            {
                HasCredentialError = false;
                CredentialErrorMessage = string.Empty;
            }
        }
    }

    partial void OnPasswordChanged(string value)
    {
        if (HasPasswordError)
        {
            HasPasswordError = false;
            PasswordErrorMessage = string.Empty;
            if (!HasEmailError)
            {
                HasCredentialError = false;
                CredentialErrorMessage = string.Empty;
            }
        }
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsBusy))
        {
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    [RelayCommand]
    private async Task Entrar()
    {
        if (IsBusy)
            return;

        // Limpa estados de erro anteriores
        HasEmailError = false;
        EmailErrorMessage = string.Empty;
        HasPasswordError = false;
        PasswordErrorMessage = string.Empty;
        HasCredentialError = false;
        CredentialErrorMessage = string.Empty;

        bool isValid = true;

        // Validação de E-mail via Validators
        if (string.IsNullOrWhiteSpace(Email))
        {
            HasEmailError = true;
            EmailErrorMessage = "Informe seu e-mail.";
            isValid = false;
        }
        else if (!Validators.IsValidEmail(Email))
        {
            HasEmailError = true;
            EmailErrorMessage = "E-mail inválido. Utilize o formato nome@empresa.com.";
            isValid = false;
        }

        // No login só exija e-mail válido e senha preenchida (sem validação de formato)
        if (string.IsNullOrWhiteSpace(Password))
        {
            HasPasswordError = true;
            PasswordErrorMessage = "Informe sua senha.";
            isValid = false;
        }

        if (!isValid)
        {
            HasCredentialError = true;
            CredentialErrorMessage = "Credenciais inválidas. Verifique os campos destacados.";
            return;
        }

        try
        {
            IsBusy = true;

            var loginResult = await _authService.LoginAsync(Email.Trim(), Password);

#if !DEBUG
            if (!string.Equals(loginResult.User.Role, "Technician", StringComparison.OrdinalIgnoreCase))
            {
                await _authService.LogoutAsync();
                HasCredentialError = true;
                CredentialErrorMessage = "Este aplicativo é exclusivo para técnicos.";
                return;
            }
#endif

            if (IPlatformApplication.Current?.Services.GetService(typeof(IAppNotificationService)) is IAppNotificationService notificationService)
            {
                _ = notificationService.RequestPermissionAsync();
            }

            await Shell.Current.GoToAsync("//main/home");
        }
        catch (ApiException ex)
        {
            HasCredentialError = true;
            if (ex.StatusCode == 400 || ex.StatusCode == 401)
            {
                CredentialErrorMessage = "E-mail ou senha inválidos.";
            }
            else
            {
                CredentialErrorMessage = ex.Message;
            }
            HasEmailError = true;
            HasPasswordError = true;
        }
        catch (Exception ex)
        {
            HasCredentialError = true;
            CredentialErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ForgotPassword()
    {
        await Shell.Current.GoToAsync(Routes.ForgotPassword);
    }
}
