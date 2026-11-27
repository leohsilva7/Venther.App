using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Venther.App.Models;
using Venther.App.Services.Api;
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
    private bool _isTool;

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
    private readonly IDeviceService _deviceService;
    private readonly IClientService _clientService;
    private readonly ITelemetryHubService _telemetryHub;
    private readonly ISyncService _syncService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _workOrderId = string.Empty;

    [ObservableProperty]
    private WorkOrder? _passedWorkOrder;

    [ObservableProperty]
    private WorkOrder? _workOrder;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _offlineMessage = string.Empty;

    [ObservableProperty]
    private bool _isPendingSync;

    // SLA / Status banner
    [ObservableProperty]
    private string _slaRemainingTime = "--";

    [ObservableProperty]
    private string _slaStatusText = "ORDEM DE SERVIÇO";

    [ObservableProperty]
    private string _priorityText = "MANUTENÇÃO";

    // Telemetry items (etapa 5 preencherá com dados reais)
    [ObservableProperty]
    private string _tempValue = "--";

    [ObservableProperty]
    private string _pressureValue = "--";

    [ObservableProperty]
    private string _deltaPValue = "--";

    // Client and contact
    [ObservableProperty]
    private string _clientName = "--";

    [ObservableProperty]
    private string _contactPerson = "--";

    [ObservableProperty]
    private string _contactRole = "Cliente";

    [ObservableProperty]
    private string _contactPhone = string.Empty;

    // Checklist local
    [ObservableProperty]
    private ObservableCollection<ChecklistItemViewModel> _checklist = new();

    [ObservableProperty]
    private int _completedChecklistCount;

    [ObservableProperty]
    private int _totalChecklistCount;

    [ObservableProperty]
    private string _checklistProgressText = "0/0 itens";

    public WorkOrderDetailViewModel(
        IWorkOrderService workOrderService,
        IDeviceService deviceService,
        IClientService clientService,
        ITelemetryHubService telemetryHub,
        ISyncService syncService,
        IDialogService dialogService)
    {
        _workOrderService = workOrderService;
        _deviceService = deviceService;
        _clientService = clientService;
        _telemetryHub = telemetryHub;
        _syncService = syncService;
        _dialogService = dialogService;
        Title = "Detalhes da O.S.";
        InitChecklist();
    }

    private void InitChecklist()
    {
        var items = new ObservableCollection<ChecklistItemViewModel>
        {
            new ChecklistItemViewModel { Name = "Válvula de Expansão TXV", Code = "VALV-TXV-410A", IsChecked = false, IsTool = false },
            new ChecklistItemViewModel { Name = "Filtro Secador 5/8", Code = "FLTR-DRY-58", IsChecked = false, IsTool = false },
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
            Title = string.IsNullOrWhiteSpace(value.Id) ? "Detalhes da O.S." : $"O.S. #{value.Id}";
            UpdateBannerFromOrder(value);
        }
    }

    partial void OnWorkOrderIdChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _ = LoadDetailsAsync(value);
        }
    }

    private void UpdateBannerFromOrder(WorkOrder order)
    {
        SlaRemainingTime = order.ScheduledDateLabel ?? order.Date.ToString("dd/MM/yyyy");
        SlaStatusText = order.Status switch
        {
            WorkOrderStatus.Done => "CONCLUÍDA",
            WorkOrderStatus.InProgress => "EM ANDAMENTO",
            _ => "AGENDADA"
        };
    }

    private async Task LoadDetailsAsync(string id)
    {
        try
        {
            IsBusy = true;

            var orderTask = _workOrderService.GetWorkOrderByIdAsync(id);
            Task<CachedResult<Models.Device?>>? boardTask = PassedWorkOrder != null && PassedWorkOrder.BoardId != Guid.Empty
                ? _deviceService.GetDeviceByIdAsync(PassedWorkOrder.BoardId.ToString())
                : null;

            if (boardTask != null)
            {
                await Task.WhenAll(orderTask, boardTask);
            }
            else
            {
                await orderTask;
            }

            var orderResult = orderTask.Result;
            var order = orderResult?.Data ?? PassedWorkOrder;
            if (order != null)
            {
                WorkOrder = order;
                Title = $"O.S. #{order.Id}";
                UpdateBannerFromOrder(order);
            }

            IsPendingSync = await _syncService.IsWorkOrderPendingSyncAsync(id);
            if (IsPendingSync)
            {
                SlaStatusText = "CONCLUÍDA • PENDENTE DE ENVIO";
            }

            if (orderResult != null && orderResult.FromCache && orderResult.CachedAt.HasValue)
            {
                IsOffline = true;
                OfflineMessage = $"Sem conexão • dados de {orderResult.CachedAt.Value:dd/MM HH:mm}";
            }
            else
            {
                IsOffline = false;
                OfflineMessage = string.Empty;
            }

            var board = boardTask?.Result?.Data;
            if (board == null && order != null && order.BoardId != Guid.Empty)
            {
                var br = await _deviceService.GetDeviceByIdAsync(order.BoardId.ToString());
                board = br.Data;
            }

            if (board != null)
            {
                ClientName = string.IsNullOrWhiteSpace(board.Location) ? "--" : board.Location;

                if (Guid.TryParse(board.Id, out var boardGuid))
                {
                    var telemetry = _telemetryHub.GetLatestTelemetry(boardGuid);
                    if (telemetry != null)
                    {
                        TempValue = telemetry.SupplyAirTemperature.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                        PressureValue = telemetry.GasLineTemperature.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                        DeltaPValue = telemetry.EvaporatorCoilPressureDrop.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                if (board.CustomerId != Guid.Empty)
                {
                    try
                    {
                        var customerResult = await _clientService.GetCustomerByIdAsync(board.CustomerId);
                        var customer = customerResult.Data;
                        if (customer != null)
                        {
                            ClientName = customer.Name;
                            ContactPerson = customer.Name;
                            ContactPhone = customer.PhoneNumber;
                        }
                    }
                    catch
                    {
                        // Se o cliente falhar, continua sem ele
                    }
                }
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
        if (string.IsNullOrWhiteSpace(ContactPhone))
        {
            await _dialogService.ShowAlertAsync("Telefone indisponível", "Não há telefone cadastrado para este cliente.", "OK", DialogType.Warning);
            return;
        }

        try
        {
            if (PhoneDialer.Default.IsSupported)
                PhoneDialer.Default.Open(ContactPhone);
        }
        catch
        {
            await _dialogService.ShowAlertAsync("Erro", "Não foi possível abrir o discador.", "OK", DialogType.Error);
        }
    }

    [RelayCommand]
    private async Task MessageClientAsync()
    {
        if (string.IsNullOrWhiteSpace(ContactPhone))
        {
            await _dialogService.ShowAlertAsync("Telefone indisponível", "Não há telefone cadastrado para este cliente.", "OK", DialogType.Warning);
            return;
        }

        try
        {
            if (Sms.Default.IsComposeSupported)
                await Sms.Default.ComposeAsync(new SmsMessage(string.Empty, ContactPhone));
        }
        catch
        {
            await _dialogService.ShowAlertAsync("Erro", "Não foi possível abrir o aplicativo de mensagens.", "OK", DialogType.Error);
        }
    }

    [RelayCommand]
    private async Task CompleteOrderAsync()
    {
        if (IsBusy || WorkOrder == null) return;

        bool confirm = await _dialogService.ShowConfirmationAsync(
            "Concluir Chamado",
            "Deseja realmente marcar esta ordem de serviço como concluída?",
            "Sim, Concluir",
            "Cancelar");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            await _workOrderService.CompleteWorkOrderAsync(WorkOrder.Id);
            WorkOrder.IsDone = true;
            WorkOrder.Status = WorkOrderStatus.Done;

            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            WorkOrder.IsDone = true;
            WorkOrder.Status = WorkOrderStatus.Done;
            IsPendingSync = true;
            SlaStatusText = "CONCLUÍDA • PENDENTE DE ENVIO";

            await _dialogService.ShowAlertAsync("Sem conexão", "Sem conexão: o checkout será enviado quando a internet voltar.", "Entendi", DialogType.Warning);
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            await _dialogService.ShowAlertAsync("Erro ao concluir", ex.Message, "OK", DialogType.Error);
        }
        catch (Exception)
        {
            await _dialogService.ShowAlertAsync("Erro ao concluir", "Não foi possível concluir a ordem de serviço.", "OK", DialogType.Error);
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
