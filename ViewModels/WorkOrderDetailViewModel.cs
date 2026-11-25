using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class ChecklistItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _isChecked;

    [ObservableProperty]
    private bool _isTool; // true for tool, false for part

    public Action? OnCheckedChanged { get; set; }

    partial void OnIsCheckedChanged(bool value)
    {
        OnCheckedChanged?.Invoke();
    }
}

[QueryProperty(nameof(WorkOrderId), "WorkOrderId")]
[QueryProperty(nameof(PassedWorkOrder), "WorkOrder")]
public partial class WorkOrderDetailViewModel : BaseViewModel
{
    private readonly IWorkOrderService _workOrderService;

    [ObservableProperty]
    private string _workOrderId = string.Empty;

    [ObservableProperty]
    private WorkOrder? _passedWorkOrder;

    [ObservableProperty]
    private WorkOrder? _workOrder;

    // SLA / Status banner
    [ObservableProperty]
    private string _slaRemainingTime = "01h 45m";

    [ObservableProperty]
    private string _slaStatusText = "SLA EM ANDAMENTO";

    [ObservableProperty]
    private string _priorityText = "ALTA PRIORIDADE";

    // Telemetry items
    [ObservableProperty]
    private string _tempValue = "32.4";

    [ObservableProperty]
    private string _pressureValue = "14.2";

    [ObservableProperty]
    private string _deltaPValue = "280";

    // Client and contact
    [ObservableProperty]
    private string _clientName = "Shopping Morumbi Prime";

    [ObservableProperty]
    private string _contactPerson = "Eng. Marcos Oliveira";

    [ObservableProperty]
    private string _contactRole = "Gerente de Facilities";

    [ObservableProperty]
    private string _contactPhone = "(11) 98765-4321";

    // Checklist
    [ObservableProperty]
    private ObservableCollection<ChecklistItemViewModel> _checklist = new();

    [ObservableProperty]
    private int _completedChecklistCount;

    [ObservableProperty]
    private int _totalChecklistCount;

    [ObservableProperty]
    private string _checklistProgressText = "0/0 itens";

    public WorkOrderDetailViewModel(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
        Title = "Detalhes da O.S.";
        InitChecklist();
    }

    private void InitChecklist()
    {
        var items = new ObservableCollection<ChecklistItemViewModel>
        {
            new ChecklistItemViewModel { Name = "Válvula de Expansão TXV", Code = "VALV-TXV-410A", IsChecked = true, IsTool = false },
            new ChecklistItemViewModel { Name = "Filtro Secador 5/8", Code = "FLTR-DRY-58", IsChecked = true, IsTool = false },
            new ChecklistItemViewModel { Name = "Manifold Digital 4 Vias", Code = "TOOL-MANIF-4V", IsChecked = false, IsTool = true },
            new ChecklistItemViewModel { Name = "Bomba de Vácuo 12 CFM", Code = "TOOL-VAC-12CFM", IsChecked = false, IsTool = true },
            new ChecklistItemViewModel { Name = "Cilindro R-410A (11.3 kg)", Code = "GAS-R410A-11", IsChecked = false, IsTool = false },
        };

        foreach (var item in items)
        {
            item.OnCheckedChanged = UpdateChecklistSummary;
        }

        Checklist = items;
        UpdateChecklistSummary();
    }

    private void UpdateChecklistSummary()
    {
        TotalChecklistCount = Checklist.Count;
        CompletedChecklistCount = Checklist.Count(i => i.IsChecked);
        ChecklistProgressText = $"{CompletedChecklistCount}/{TotalChecklistCount} concluídos";
    }

    partial void OnPassedWorkOrderChanged(WorkOrder? value)
    {
        if (value != null)
        {
            WorkOrder = value;
            Title = string.IsNullOrWhiteSpace(value.Id) ? "Detalhes da O.S." : value.Id;
        }
    }

    partial void OnWorkOrderIdChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && (WorkOrder == null || WorkOrder.Id != value))
        {
            _ = LoadWorkOrderAsync(value);
        }
    }

    private async Task LoadWorkOrderAsync(string id)
    {
        try
        {
            IsBusy = true;
            var order = await _workOrderService.GetWorkOrderByIdAsync(id);
            if (order != null)
            {
                WorkOrder = order;
                Title = order.Id;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CallClientAsync()
    {
        if (Application.Current?.Windows.Count > 0)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync("Ligar para Cliente", $"Ligando para {ContactPerson} ({ContactPhone})...", "OK");
        }
    }

    [RelayCommand]
    private async Task MessageClientAsync()
    {
        if (Application.Current?.Windows.Count > 0)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync("Enviar Mensagem", $"Iniciando chat com {ContactPerson}...", "OK");
        }
    }

    [RelayCommand]
    private async Task CompleteOrderAsync()
    {
        if (IsBusy) return;

        var page = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
        if (page != null)
        {
            bool confirm = await page.DisplayAlertAsync("Concluir Chamado", "Deseja realmente marcar esta ordem de serviço como concluída?", "Sim, Concluir", "Cancelar");
            if (!confirm) return;
        }

        try
        {
            IsBusy = true;
            if (WorkOrder != null)
            {
                await _workOrderService.CompleteWorkOrderAsync(WorkOrder.Id);
                WorkOrder.IsDone = true;
                WorkOrder.Status = WorkOrderStatus.Done;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
