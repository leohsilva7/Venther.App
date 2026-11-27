using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.Controls;

public partial class ProfileMenuPopup : Popup
{
    private readonly ISessionService? _sessionService;
    private readonly IAuthService? _authService;
    private bool _isLoggingOut;

    public ProfileMenuPopup()
    {
        InitializeComponent();

        _sessionService = IPlatformApplication.Current?.Services.GetService(typeof(ISessionService)) as ISessionService;
        _authService = IPlatformApplication.Current?.Services.GetService(typeof(IAuthService)) as IAuthService;

        LoadUserData();
    }

    private void LoadUserData()
    {
        var user = _sessionService?.CurrentUser;
        var name = !string.IsNullOrWhiteSpace(user?.Name) ? user.Name.Trim() : "Técnico";
        var email = !string.IsNullOrWhiteSpace(user?.Email) ? user.Email.Trim() : "tecnico@venther.com";
        var role = !string.IsNullOrWhiteSpace(user?.Role) ? user.Role.Trim() : "Técnico";

        NameLabel.Text = name;
        EmailLabel.Text = email;
        RoleLabel.Text = role.ToUpperInvariant();

        InitialsLabel.Text = GetInitials(name);
    }

    private static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "TE";

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0][..2].ToUpperInvariant()
                : parts[0].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }

    private void OnPromptLogoutTapped(object? sender, EventArgs e)
    {
        ProfileMenuLayout.IsVisible = false;
        ConfirmLogoutLayout.IsVisible = true;
    }

    private void OnBackToProfileTapped(object? sender, EventArgs e)
    {
        if (_isLoggingOut) return;

        ConfirmLogoutLayout.IsVisible = false;
        ProfileMenuLayout.IsVisible = true;
    }

    private void OnCloseTapped(object? sender, EventArgs e)
    {
        if (_isLoggingOut) return;

        Close();
    }

    private async void OnConfirmLogoutTapped(object? sender, EventArgs e)
    {
        if (_isLoggingOut) return;

        _isLoggingOut = true;
        ConfirmLogoutText.IsVisible = false;
        LogoutIndicator.IsVisible = true;
        LogoutIndicator.IsRunning = true;
        BackButton.Opacity = 0.5;

        try
        {
            if (_authService != null)
            {
                await _authService.LogoutAsync();
            }
            else if (_sessionService != null)
            {
                await _sessionService.ClearAsync();
            }
        }
        catch
        {
            // Ignora falhas da API no logout e garante a navegação
        }
        finally
        {
            try
            {
                Close();
            }
            catch
            {
                // Ignora se o popup já estiver fechando
            }

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("//login");
            }
        }
    }
}
