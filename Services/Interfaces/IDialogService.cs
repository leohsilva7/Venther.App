using System.Threading.Tasks;

namespace Venther.App.Services.Interfaces;

public enum DialogType
{
    Info,
    Success,
    Warning,
    Error
}

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string buttonText = "OK", DialogType type = DialogType.Info);
    Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "Confirmar", string cancelText = "Cancelar", bool isDestructive = false);
    Task<string?> ShowActionSheetAsync(string title, string cancelText, params string[] options);
    Task<string?> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancelar", string? initialValue = null, string? placeholder = null);
}
