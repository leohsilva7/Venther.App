using Venther.App.ViewModels;

namespace Venther.App.Views;

public partial class RegisterDevicePage : ContentPage
{
    public RegisterDevicePage(RegisterDeviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Construtor sem parâmetros caso a navegação via Shell instancie diretamente sem DI
    public RegisterDevicePage() : this(new RegisterDeviceViewModel())
    {
    }
}