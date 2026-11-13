using Venther.App.ViewModels;

namespace Venther.App.Views.Alerts;

public partial class AlertsPage : ContentPage
{
    private readonly AlertsViewModel _vm;

    public AlertsPage(AlertsViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshCommand.Execute(null);
    }
}
