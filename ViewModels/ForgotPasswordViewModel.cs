using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _hasEmailError;

    [ObservableProperty]
    private string _emailErrorMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Recuperar Senha";
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsBusy))
        {
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    partial void OnEmailChanged(string value)
    {
        if (HasEmailError)
        {
            HasEmailError = false;
            EmailErrorMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SendInstructionsAsync()
    {
        if (IsBusy)
            return;

        // Limpa erros anteriores
        HasEmailError = false;
        EmailErrorMessage = string.Empty;

        // Validação com Validators.IsValidEmail
        if (string.IsNullOrWhiteSpace(Email))
        {
            HasEmailError = true;
            EmailErrorMessage = "Informe seu endereço de e-mail.";
            return;
        }

        if (!Validators.IsValidEmail(Email))
        {
            HasEmailError = true;
            EmailErrorMessage = "Insira um endereço de e-mail válido.";
            return;
        }

        IsBusy = true;
        try
        {
            await _authService.ForgotPasswordAsync(Email.Trim());

            // Navega para CheckEmailPage passando o e-mail
            var navParams = new Dictionary<string, object>
            {
                ["Email"] = Email.Trim()
            };

            await Shell.Current.GoToAsync(Routes.CheckEmail, navParams);
        }
        catch (ApiException ex)
        {
            HasEmailError = true;
            EmailErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasEmailError = true;
            EmailErrorMessage = "Não foi possível enviar o e-mail de recuperação. Tente novamente.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
