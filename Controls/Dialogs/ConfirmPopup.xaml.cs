using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace Venther.App.Controls.Dialogs;

public partial class ConfirmPopup : Popup
{
    public ConfirmPopup(string title, string message, string confirmText = "Confirmar", string cancelText = "Cancelar", bool isDestructive = false)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ConfirmText.Text = confirmText;
        CancelText.Text = cancelText;

        if (isDestructive)
        {
            ConfirmButton.BackgroundColor = Color.FromArgb("#BA1A1A"); // Error
            IconBadge.BackgroundColor = Color.FromArgb("#FEF2F2"); // ErrorBg
            IconImage.Source = "alert_triangle.svg";
            IconTint.TintColor = Color.FromArgb("#BA1A1A"); // Error
        }
        else
        {
            ConfirmButton.BackgroundColor = Color.FromArgb("#00326F"); // Primary
            IconBadge.BackgroundColor = Color.FromArgb("#EEF4FC"); // Surface
            IconImage.Source = "check_circle.svg";
            IconTint.TintColor = Color.FromArgb("#00326F"); // Primary
        }
    }

    private void OnConfirmTapped(object? sender, EventArgs e)
    {
        Close(true);
    }

    private void OnCancelTapped(object? sender, EventArgs e)
    {
        Close(false);
    }
}
