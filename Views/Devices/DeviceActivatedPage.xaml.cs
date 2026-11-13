using Venther.App.ViewModels;

namespace Venther.App.Views.Devices;

public partial class DeviceActivatedPage : ContentPage
{
    public DeviceActivatedPage(DeviceActivatedViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
