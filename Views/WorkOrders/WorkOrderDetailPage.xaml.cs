using Venther.App.ViewModels;

namespace Venther.App.Views.WorkOrders;

public partial class WorkOrderDetailPage : ContentPage
{
    public WorkOrderDetailPage(WorkOrderDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
