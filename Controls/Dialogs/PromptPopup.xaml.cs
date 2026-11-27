using System;
using CommunityToolkit.Maui.Views;

namespace Venther.App.Controls.Dialogs;

public partial class PromptPopup : Popup
{
    public PromptPopup(string title, string message, string acceptText = "OK", string cancelText = "Cancelar", string? initialValue = null, string? placeholder = null)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        AcceptText.Text = acceptText;
        CancelText.Text = cancelText;

        if (!string.IsNullOrEmpty(initialValue))
        {
            InputEntry.Text = initialValue;
        }

        if (!string.IsNullOrEmpty(placeholder))
        {
            InputEntry.Placeholder = placeholder;
        }
    }

    private void OnAcceptTapped(object? sender, EventArgs e)
    {
        Close(InputEntry.Text ?? string.Empty);
    }

    private void OnCancelTapped(object? sender, EventArgs e)
    {
        Close(null);
    }
}
