using Venther.App.ViewModels;

namespace Venther.App.Views.Devices;

public partial class DeviceDetailPage : ContentPage
{
    public DeviceDetailPage(DeviceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
