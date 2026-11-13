using Venther.App.ViewModels;

namespace Venther.App.Views.Devices;

public partial class RegisterDevicePage : ContentPage
{
    public RegisterDevicePage(RegisterDeviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
