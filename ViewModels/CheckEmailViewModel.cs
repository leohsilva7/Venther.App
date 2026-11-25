using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Venther.App.Helpers;

namespace Venther.App.ViewModels;

[QueryProperty(nameof(Email), "Email")]
public partial class CheckEmailViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _email = string.Empty;

    public CheckEmailViewModel()
    {
        Title = "Verificar E-mail";
    }

    [RelayCommand]
    private async Task OpenEmailAppAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            // Mock advance to ResetPasswordPage passing email
            await Shell.Current.GoToAsync(Routes.ResetPassword, new Dictionary<string, object>
            {
                ["Email"] = Email
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsBusy) return;

        // Returns to Login or previous page
        await Shell.Current.GoToAsync("..");
    }
}
