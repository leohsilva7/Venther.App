using Venther.App.ViewModels;

namespace Venther.App.Views.Devices;

public partial class DeviceConnectionPage : ContentPage
{
    public DeviceConnectionPage(DeviceConnectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
