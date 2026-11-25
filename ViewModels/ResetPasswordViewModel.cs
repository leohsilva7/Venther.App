using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Venther.App.Helpers;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(Email), "Email")]
[QueryProperty(nameof(Code), "Code")]
public partial class ResetPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _hasMinLength;

    [ObservableProperty]
    private bool _hasUppercase;

    [ObservableProperty]
    private bool _hasLowercase;

    [ObservableProperty]
    private bool _hasNumber;

    [ObservableProperty]
    private bool _hasSpecialChar;

    [ObservableProperty]
    private bool _isPasswordValid;

    [ObservableProperty]
    private bool _passwordsMatch;

    [ObservableProperty]
    private bool _showMatchIndicator;

    [ObservableProperty]
    private bool _canSubmit;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Password strength: 0 to 4 segments
    [ObservableProperty]
    private int _strengthLevel;

    [ObservableProperty]
    private string _strengthText = "Muito fraca";

    [ObservableProperty]
    private Color _strengthColor = Colors.Transparent;

    [ObservableProperty]
    private bool _isSegment1Active;

    [ObservableProperty]
    private bool _isSegment2Active;

    [ObservableProperty]
    private bool _isSegment3Active;

    [ObservableProperty]
    private bool _isSegment4Active;

    public ResetPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Redefinir Senha";
        UpdateStrength();
    }

    partial void OnNewPasswordChanged(string value)
    {
        ValidatePasswordRules();
        CheckPasswordsMatch();
        UpdateStrength();
        UpdateCanSubmit();
        HasError = false;
        ErrorMessage = string.Empty;
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        CheckPasswordsMatch();
        UpdateCanSubmit();
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void ValidatePasswordRules()
    {
        var pwd = NewPassword ?? string.Empty;
        HasMinLength = pwd.Length >= 8 && pwd.Length <= 100;
        HasUppercase = pwd.Any(char.IsUpper);
        HasLowercase = pwd.Any(char.IsLower);
        HasNumber = pwd.Any(char.IsDigit);
        HasSpecialChar = pwd.Any(ch => !char.IsLetterOrDigit(ch));

        IsPasswordValid = Validators.IsValidPassword(pwd);
    }

    private void CheckPasswordsMatch()
    {
        var hasConfirm = !string.IsNullOrEmpty(ConfirmPassword);
        var hasPwd = !string.IsNullOrEmpty(NewPassword);

        ShowMatchIndicator = hasConfirm;
        PasswordsMatch = hasConfirm && hasPwd && string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal);
    }

    private void UpdateStrength()
    {
        var pwd = NewPassword ?? string.Empty;

        if (string.IsNullOrEmpty(pwd))
        {
            StrengthLevel = 0;
            StrengthText = "Fraca";
            StrengthColor = Color.FromArgb("#64748B"); // AuthMuted
            IsSegment1Active = false;
            IsSegment2Active = false;
            IsSegment3Active = false;
            IsSegment4Active = false;
            return;
        }

        int score = 0;
        if (pwd.Length >= 8) score++;
        if (pwd.Any(char.IsUpper) && pwd.Any(char.IsLower)) score++;
        if (pwd.Any(char.IsDigit)) score++;
        if (pwd.Any(ch => !char.IsLetterOrDigit(ch))) score++;

        StrengthLevel = score;
        IsSegment1Active = score >= 1;
        IsSegment2Active = score >= 2;
        IsSegment3Active = score >= 3;
        IsSegment4Active = score >= 4;

        switch (score)
        {
            case 1:
                StrengthText = "Fraca";
                StrengthColor = Color.FromArgb("#BA1A1A"); // Error
                break;
            case 2:
                StrengthText = "Razoável";
                StrengthColor = Color.FromArgb("#FC6100"); // Accent
                break;
            case 3:
                StrengthText = "Boa";
                StrengthColor = Color.FromArgb("#034899"); // LogoBlue / Primary
                break;
            case 4:
                StrengthText = "Forte";
                StrengthColor = Color.FromArgb("#10B981"); // Success
                break;
            default:
                StrengthText = "Muito fraca";
                StrengthColor = Color.FromArgb("#BA1A1A"); // Error
                break;
        }
    }

    private void UpdateCanSubmit()
    {
        CanSubmit = IsPasswordValid && PasswordsMatch && !string.IsNullOrWhiteSpace(Code) && !IsBusy;
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (!CanSubmit || IsBusy) return;

        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            IsBusy = true;
            UpdateCanSubmit();

            await _authService.ResetPasswordAsync(Email.Trim(), Code.Trim(), NewPassword, ConfirmPassword);

            await Shell.Current.GoToAsync(Routes.PasswordResetSuccess);
        }
        catch (ApiException ex)
        {
            HasError = true;
            if (ex.StatusCode == 400 && ex.Message.Contains("code", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Código de verificação inválido ou expirado.";
            }
            else
            {
                ErrorMessage = ex.Message;
            }
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Não foi possível redefinir a senha. Tente novamente.";
        }
        finally
        {
            IsBusy = false;
            UpdateCanSubmit();
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsBusy) return;
        await Shell.Current.GoToAsync("..");
    }
}
