using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class WorkOrdersViewModel : BaseViewModel
{
    private const int PageSize = 20;
    private readonly IWorkOrderService _workOrderService;
    private readonly ISyncService _syncService;
    private readonly List<WorkOrder> _allWorkOrders = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _isLoadingMore;

    [ObservableProperty]
    private ObservableCollection<WorkOrder> _filteredWorkOrders = new();

    [ObservableProperty]
    private ObservableCollection<SegmentItem> _filterSegments = new();

    [ObservableProperty]
    private int _selectedSegmentIndex;

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _offlineMessage = string.Empty;

    public WorkOrdersViewModel(IWorkOrderService workOrderService, ISyncService syncService)
    {
        _workOrderService = workOrderService;
        _syncService = syncService;
        Title = "Chamados";
        InitSegments();
    }

    private void InitSegments()
    {
        FilterSegments = new ObservableCollection<SegmentItem>
        {
            new SegmentItem("Todas", 0, "all"),
            new SegmentItem("Em andamento", 0, "inprogress"),
            new SegmentItem("Agendadas", 0, "scheduled"),
            new SegmentItem("Concluídas", 0, "done"),
        };
    }

    partial void OnSelectedSegmentIndexChanged(int value) => ApplyFilter();

    [RelayCommand]
    public async Task Refresh()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        _currentPage = 1;

        try
        {
            _ = _syncService.ProcessQueueAsync();

            var result = await _workOrderService.GetWorkOrdersAsync(_currentPage, PageSize);
            _allWorkOrders.Clear();
            _allWorkOrders.AddRange(result.Data.Items);
            _totalPages = Math.Max(1, result.Data.TotalPages);

            IsOffline = result.FromCache;
            if (result.FromCache && result.CachedAt.HasValue)
            {
                OfflineMessage = $"Sem conexão • dados de {result.CachedAt.Value:dd/MM HH:mm}";
            }
            else
            {
                OfflineMessage = string.Empty;
            }

            UpdateCounts();
            ApplyFilter();
        }
        catch (ApiException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Não foi possível carregar as ordens de serviço.";
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || _isLoadingMore || _currentPage >= _totalPages)
            return;

        try
        {
            _isLoadingMore = true;
            var nextPage = _currentPage + 1;
            var result = await _workOrderService.GetWorkOrdersAsync(nextPage, PageSize);

            if (result.Data.Items.Count > 0)
            {
                _currentPage = nextPage;
                _totalPages = Math.Max(1, result.Data.TotalPages);
                _allWorkOrders.AddRange(result.Data.Items);
                UpdateCounts();
                ApplyFilter();
            }
        }
        catch
        {
            // Mantém os itens já carregados caso a paginação falhe
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    private void UpdateCounts()
    {
        FilterSegments[0].Count = _allWorkOrders.Count;
        FilterSegments[1].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.InProgress);
        FilterSegments[2].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.Scheduled);
        FilterSegments[3].Count = _allWorkOrders.Count(w => w.Status == WorkOrderStatus.Done);

        ActiveCount = FilterSegments[1].Count + FilterSegments[2].Count;
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
