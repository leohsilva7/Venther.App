using Microsoft.Maui.Controls;
using Venther.App.ViewModels;

namespace Venther.App.Views.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel vm)
        {
            _ = vm.LoadDevicesAsync();
        }
    }
}
