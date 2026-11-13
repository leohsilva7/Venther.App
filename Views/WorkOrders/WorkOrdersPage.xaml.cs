using Venther.App.ViewModels;

namespace Venther.App.Views.WorkOrders;

public partial class WorkOrdersPage : ContentPage
{
    private readonly WorkOrdersViewModel _vm;

    public WorkOrdersPage(WorkOrdersViewModel viewModel)
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
