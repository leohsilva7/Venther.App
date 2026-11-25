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

    public WorkOrderService(IApiClient apiClient, ISessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    public async Task<PagedResult<WorkOrder>> GetWorkOrdersAsync(int page = 1, int pageSize = 20)
    {
        var userId = _sessionService.CurrentUser?.Id;
        var query = userId.HasValue
            ? $"/maintence_requests?page={page}&pageSize={pageSize}&userId={userId.Value}"
            : $"/maintence_requests?page={page}&pageSize={pageSize}";

        var result = await _apiClient.GetAsync<PagedResult<MaintenanceRequestDto>>(query);
        if (result == null)
        {
            return new PagedResult<WorkOrder>(Array.Empty<WorkOrder>(), page, pageSize, 0, 0);
        }

        var items = result.Items.Select(MapWorkOrder).ToList();
        return new PagedResult<WorkOrder>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);
    }

    public async Task<WorkOrder?> GetWorkOrderByIdAsync(string id)
    {
        if (!int.TryParse(id, out var requestId))
            return null;

        var dto = await _apiClient.GetAsync<MaintenanceRequestDto>($"/maintence_requests/{requestId}");
        return dto == null ? null : MapWorkOrder(dto);
    }

    public async Task<bool> CompleteWorkOrderAsync(string id)
    {
        if (!int.TryParse(id, out var requestId))
            return false;

        await _apiClient.PutAsync($"/maintence_requests/{requestId}", new CompleteMaintenanceRequestDto(true));
        return true;
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
