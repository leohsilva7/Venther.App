using System;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls;
using Venther.App.Helpers;
using Venther.App.Services.Interfaces;
using Venther.App.ViewModels;

namespace Venther.App.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

#if DEBUG
        var touchBehavior = new TouchBehavior();
        touchBehavior.LongPressCompleted += OnLogoLongPressed;
        LogoBorder.Behaviors.Add(touchBehavior);
#endif
    }

#if DEBUG
    private async void OnLogoLongPressed(object? sender, EventArgs e)
    {
        var dialogService = IPlatformApplication.Current?.Services.GetService(typeof(IDialogService)) as IDialogService;
        if (dialogService == null) return;

        var result = await dialogService.ShowPromptAsync(
            "URL do Servidor",
            "Informe a URL base da API (sem barra no final):",
            acceptText: "Salvar",
            cancelText: "Cancelar",
            initialValue: ApiConfig.BaseUrl,
            placeholder: "http://10.0.2.2:5084");

        if (result == null)
            return;

        var url = result.Trim();
        if ((!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
             !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ||
            url.EndsWith('/'))
        {
            await dialogService.ShowAlertAsync("URL Inválida", "A URL deve começar com http:// ou https:// e não pode terminar com \"/\".", "OK", DialogType.Error);
            return;
        }

        ApiConfig.SetBaseUrl(url);

        if (IPlatformApplication.Current?.Services.GetService(typeof(ITelemetryHubService)) is ITelemetryHubService hub)
        {
            await hub.DisconnectAsync();
        }
    }
#endif
}
