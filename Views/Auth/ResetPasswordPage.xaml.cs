using Venther.App.ViewModels;

namespace Venther.App.Views.Auth;

public partial class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(ResetPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
