using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Venther.App.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class WorkOrdersViewModel : BaseViewModel
{
    private readonly IWorkOrderService _workOrderService;
    private List<WorkOrder> _allWorkOrders = new();

    [ObservableProperty]
    private ObservableCollection<WorkOrder> _filteredWorkOrders = new();

    [ObservableProperty]
    private ObservableCollection<SegmentItem> _filterSegments = new();

    [ObservableProperty]
    private int _selectedSegmentIndex;

    [ObservableProperty]
    private int _activeCount;

    public WorkOrdersViewModel(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
        Title = "Chamados";
        InitSegments();
    }

    private void InitSegments()
    {
        FilterSegments = new ObservableCollection<SegmentItem>
        {
            new SegmentItem("Todas",      0, "all"),
            new SegmentItem("Em andamento", 0, "inprogress"),
            new SegmentItem("Agendadas",  0, "scheduled"),
            new SegmentItem("Concluídas", 0, "done"),
        };
    }

    partial void OnSelectedSegmentIndexChanged(int value) => ApplyFilter();

    [RelayCommand]
    private async Task Refresh()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var orders = await _workOrderService.GetWorkOrdersAsync();
            _allWorkOrders = orders.ToList();

            FilterSegments[0].Count = _allWorkOrders.Count;
            FilterSegments[1].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.InProgress);
            FilterSegments[2].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.Scheduled);
            FilterSegments[3].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.Done);

            ActiveCount = FilterSegments[1].Count + FilterSegments[2].Count;

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var key = SelectedSegmentIndex >= 0 && SelectedSegmentIndex < FilterSegments.Count
            ? FilterSegments[SelectedSegmentIndex].Key
            : "all";

        var filtered = key switch
        {
            "inprogress" => _allWorkOrders.Where(w => w.Status == WorkOrderStatus.InProgress),
            "scheduled"  => _allWorkOrders.Where(w => w.Status == WorkOrderStatus.Scheduled),
            "done"       => _allWorkOrders.Where(w => w.Status == WorkOrderStatus.Done),
            _            => _allWorkOrders.AsEnumerable(),
        };

        FilteredWorkOrders = new ObservableCollection<WorkOrder>(filtered);
    }

    [RelayCommand]
    private async Task ViewDetail(WorkOrder? workOrder)
    {
        if (workOrder == null) return;

        await Shell.Current.GoToAsync(Routes.WorkOrderDetail, new Dictionary<string, object>
        {
            ["WorkOrderId"] = workOrder.Id,
            ["WorkOrder"] = workOrder
        });
    }

    // TODO: Botão de filtro avançado — abre bottom-sheet com opções de ordenação e filtro por período.
    [RelayCommand]
    private void OpenFilter()
    {
        // TODO: implementar filtro avançado (bottom-sheet ou modal)
    }
}
