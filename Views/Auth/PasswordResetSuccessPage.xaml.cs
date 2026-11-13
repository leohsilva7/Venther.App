using Venther.App.ViewModels;

namespace Venther.App.Views.Auth;

public partial class PasswordResetSuccessPage : ContentPage
{
    public PasswordResetSuccessPage(PasswordResetSuccessViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
