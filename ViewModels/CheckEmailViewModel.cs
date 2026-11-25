using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(Email), "Email")]
public partial class CheckEmailViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _hasCodeError;

    [ObservableProperty]
    private string _codeErrorMessage = string.Empty;

    [ObservableProperty]
    private bool _isResending;

    public bool IsNotBusy => !IsBusy;

    public CheckEmailViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Verificar E-mail";
    }

    partial void OnCodeChanged(string value)
    {
        if (value.Length > 6)
        {
            Code = value.Substring(0, 6);
            return;
        }

        if (HasCodeError)
        {
            HasCodeError = false;
            CodeErrorMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (IsBusy) return;

        HasCodeError = false;
        CodeErrorMessage = string.Empty;

        var trimmedCode = Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedCode))
        {
            HasCodeError = true;
            CodeErrorMessage = "Informe o código de 6 dígitos recebido.";
            return;
        }

        if (trimmedCode.Length < 6)
        {
            HasCodeError = true;
            CodeErrorMessage = "O código deve conter 6 dígitos.";
            return;
        }

        try
        {
            IsBusy = true;
            await Shell.Current.GoToAsync(Routes.ResetPassword, new Dictionary<string, object>
            {
                ["Email"] = Email,
                ["Code"] = trimmedCode
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResendEmailAsync()
    {
        if (IsBusy || IsResending || string.IsNullOrWhiteSpace(Email)) return;

        try
        {
            IsResending = true;
            await _authService.ForgotPasswordAsync(Email.Trim());

            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlertAsync("Código Reenviado", "Um novo código de 6 dígitos foi enviado para o seu e-mail.", "OK");
            }
        }
        catch (ApiException ex)
        {
            HasCodeError = true;
            CodeErrorMessage = ex.Message;
        }
        catch
        {
            HasCodeError = true;
            CodeErrorMessage = "Erro ao reenviar o código. Tente novamente.";
        }
        finally
        {
            IsResending = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsBusy) return;
        await Shell.Current.GoToAsync("..");
    }
}
