using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IApiClient _apiClient;
    private readonly ISessionService _sessionService;
    private readonly ICacheService _cacheService;
    private readonly ISyncService _syncService;

    public WorkOrderService(
        IApiClient apiClient,
        ISessionService sessionService,
        ICacheService cacheService,
        ISyncService syncService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
        _cacheService = cacheService;
        _syncService = syncService;
    }

    public async Task<CachedResult<PagedResult<WorkOrder>>> GetWorkOrdersAsync(int page = 1, int pageSize = 20)
    {
        var userId = _sessionService.CurrentUser?.Id;
        var query = userId.HasValue
            ? $"/maintence_requests?page={page}&pageSize={pageSize}&userId={userId.Value}"
            : $"/maintence_requests?page={page}&pageSize={pageSize}";

        try
        {
            var result = await _apiClient.GetAsync<PagedResult<MaintenanceRequestDto>>(query);
            if (result == null)
            {
                var empty = new PagedResult<WorkOrder>(Array.Empty<WorkOrder>(), page, pageSize, 0, 0);
                return CachedResult<PagedResult<WorkOrder>>.Fresh(empty);
            }

            var items = result.Items.Select(MapWorkOrder).ToList();
            await ApplyPendingQueueStatusAsync(items);

            var paged = new PagedResult<WorkOrder>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);

            if (page == 1)
            {
                await _cacheService.SaveAsync("workorders", paged);
            }

            return CachedResult<PagedResult<WorkOrder>>.Fresh(paged);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<PagedResult<WorkOrder>>("workorders");
            if (cached != null)
            {
                await ApplyPendingQueueStatusAsync(cached.Data.Items);
                return CachedResult<PagedResult<WorkOrder>>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    public async Task<CachedResult<WorkOrder?>> GetWorkOrderByIdAsync(string id)
    {
        if (!int.TryParse(id, out var requestId))
            return CachedResult<WorkOrder?>.Fresh(null);

        var cacheKey = $"workorder:{id}";

        try
        {
            var dto = await _apiClient.GetAsync<MaintenanceRequestDto>($"/maintence_requests/{requestId}");
            var wo = dto == null ? null : MapWorkOrder(dto);
            if (wo != null)
            {
                if (await _syncService.IsWorkOrderPendingSyncAsync(wo.Id))
                {
                    wo.IsDone = true;
                    wo.Status = WorkOrderStatus.Done;
                }
                await _cacheService.SaveAsync(cacheKey, wo);
            }
            return CachedResult<WorkOrder?>.Fresh(wo);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<WorkOrder>(cacheKey);
            if (cached != null)
            {
                if (await _syncService.IsWorkOrderPendingSyncAsync(cached.Data.Id))
                {
                    cached.Data.IsDone = true;
                    cached.Data.Status = WorkOrderStatus.Done;
                }
                return CachedResult<WorkOrder?>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    public async Task<bool> CompleteWorkOrderAsync(string id)
    {
        if (!int.TryParse(id, out var requestId))
            return false;

        try
        {
            await _apiClient.PutAsync($"/maintence_requests/{requestId}", new CompleteMaintenanceRequestDto(true));
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            // Erro de rede: enfileira para sincronização posterior
            await _syncService.EnqueueCheckoutAsync(id);
            throw;
        }
    }

    private async Task ApplyPendingQueueStatusAsync(IEnumerable<WorkOrder> workOrders)
    {
        var pendingQueue = await _syncService.GetQueueAsync();
        if (pendingQueue.Count == 0)
            return;

        var pendingIds = new HashSet<string>(pendingQueue.Select(p => p.WorkOrderId));
        foreach (var wo in workOrders)
        {
            if (pendingIds.Contains(wo.Id))
            {
                wo.IsDone = true;
                wo.Status = WorkOrderStatus.Done;
            }
        }
    }

    private static WorkOrder MapWorkOrder(MaintenanceRequestDto dto)
    {
        var date = DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.Today;

        var status = dto.IsDone
            ? WorkOrderStatus.Done
            : (date.Date > DateTime.Today ? WorkOrderStatus.Scheduled : WorkOrderStatus.InProgress);

        string scheduledLabel;
        if (date.Date == DateTime.Today)
            scheduledLabel = "Hoje";
        else if (date.Date == DateTime.Today.AddDays(1))
            scheduledLabel = "Amanhã";
        else
            scheduledLabel = date.ToString("dd/MM/yyyy");

        return new WorkOrder
        {
            Id = dto.Id.ToString(),
            BoardId = dto.BoardId,
            BoardMacAddress = dto.BoardMacAddress,
            Title = "Manutenção preventiva",
            DeviceName = dto.BoardLocation,
            Location = dto.BoardLocation,
            Date = date,
            IsDone = dto.IsDone,
            Status = status,
            TechnicianName = dto.UserName,
            ScheduledDateLabel = scheduledLabel
        };
    }
}
