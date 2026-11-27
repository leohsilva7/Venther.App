using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Venther.App.Controls.Dialogs;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class DialogService : IDialogService
{
    private static Page? CurrentPage =>
        Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;

    public async Task ShowAlertAsync(string title, string message, string buttonText = "OK", DialogType type = DialogType.Info)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = CurrentPage;
            if (page == null) return;

            var popup = new AlertPopup(title, message, buttonText, type);
            await page.ShowPopupAsync(popup);
        });
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "Confirmar", string cancelText = "Cancelar", bool isDestructive = false)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = CurrentPage;
            if (page == null) return false;

            var popup = new ConfirmPopup(title, message, confirmText, cancelText, isDestructive);
            var result = await page.ShowPopupAsync(popup);
            return result is bool b && b;
        });
    }

    public async Task<string?> ShowActionSheetAsync(string title, string cancelText, params string[] options)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = CurrentPage;
            if (page == null) return null;

            var popup = new ActionSheetPopup(title, cancelText, options);
            var result = await page.ShowPopupAsync(popup);
            return result as string;
        });
    }

    public async Task<string?> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancelar", string? initialValue = null, string? placeholder = null)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = CurrentPage;
            if (page == null) return null;

            var popup = new PromptPopup(title, message, acceptText, cancelText, initialValue, placeholder);
            var result = await page.ShowPopupAsync(popup);
            return result as string;
        });
    }
}
