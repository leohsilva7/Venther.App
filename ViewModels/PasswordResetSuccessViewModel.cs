using CommunityToolkit.Mvvm.Input;

namespace Venther.App.ViewModels;

public partial class PasswordResetSuccessViewModel : BaseViewModel
{
    public PasswordResetSuccessViewModel()
    {
        Title = "Senha Redefinida";
    }

    [RelayCommand]
    private async Task BackToLoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            // GoToAsync("//login") cleans the navigation stack and directs user to login
            await Shell.Current.GoToAsync("//login");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
