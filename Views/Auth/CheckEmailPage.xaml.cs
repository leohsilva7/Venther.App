using Venther.App.ViewModels;

namespace Venther.App.Views.Auth;

public partial class CheckEmailPage : ContentPage
{
    public CheckEmailPage(CheckEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
