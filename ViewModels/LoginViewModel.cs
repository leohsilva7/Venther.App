using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
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

        // Validação de Senha via Validators
        if (string.IsNullOrWhiteSpace(Password))
        {
            HasPasswordError = true;
            PasswordErrorMessage = "Informe sua senha.";
            isValid = false;
        }
        else if (!Validators.IsValidPassword(Password))
        {
            HasPasswordError = true;
            PasswordErrorMessage = "Mínimo 8 caracteres, 1 maiúscula e 1 caractere especial.";
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

            var success = await _authService.LoginAsync(Email, Password);

            if (success)
            {
                await Shell.Current.GoToAsync("//main/home");
            }
            else
            {
                HasCredentialError = true;
                CredentialErrorMessage = "E-mail ou senha incorretos. Verifique suas credenciais.";
                HasEmailError = true;
                HasPasswordError = true;
            }
        }
        catch (Exception)
        {
            HasCredentialError = true;
            CredentialErrorMessage = "Erro de conexão ao autenticar. Tente novamente.";
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
