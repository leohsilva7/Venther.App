using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Venther.App.Services.Interfaces;

namespace Venther.App.Controls.Dialogs;

public partial class AlertPopup : Popup
{
    public AlertPopup(string title, string message, string buttonText = "OK", DialogType type = DialogType.Info)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ButtonLabel.Text = buttonText;

        ApplyType(type);
    }

    private void ApplyType(DialogType type)
    {
        switch (type)
        {
            case DialogType.Success:
                IconBadge.BackgroundColor = Color.FromArgb("#D1FAE5"); // SuccessContainer
                IconImage.Source = "check_circle.svg";
                IconTint.TintColor = Color.FromArgb("#059669"); // SuccessText
                break;
            case DialogType.Warning:
                IconBadge.BackgroundColor = Color.FromArgb("#FFDBCD"); // AccentContainer
                IconImage.Source = "alert_triangle.svg";
                IconTint.TintColor = Color.FromArgb("#FC6100"); // Accent
                break;
            case DialogType.Error:
                IconBadge.BackgroundColor = Color.FromArgb("#FEF2F2"); // ErrorBg
                IconImage.Source = "alert_circle.svg";
                IconTint.TintColor = Color.FromArgb("#BA1A1A"); // Error
                break;
            case DialogType.Info:
            default:
                IconBadge.BackgroundColor = Color.FromArgb("#EEF4FC"); // Surface
                IconImage.Source = "alert_circle.svg";
                IconTint.TintColor = Color.FromArgb("#00326F"); // Primary
                break;
        }
    }

    private void OnConfirmTapped(object? sender, EventArgs e)
    {
        Close();
    }
}
