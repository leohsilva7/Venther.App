using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class AlertService : IAlertService
{
    private readonly IApiClient _apiClient;
    private readonly IDeviceService _deviceService;
    private Dictionary<Guid, string>? _boardLocationCache;

    public AlertService(IApiClient apiClient, IDeviceService deviceService)
    {
        _apiClient = apiClient;
        _deviceService = deviceService;
    }

    public async Task<PagedResult<AlertItem>> GetAlertsAsync(int page = 1, int pageSize = 20)
    {
        var result = await _apiClient.GetAsync<PagedResult<FailureDto>>($"/failures?page={page}&pageSize={pageSize}");
        if (result == null)
        {
            return new PagedResult<AlertItem>(Array.Empty<AlertItem>(), page, pageSize, 0, 0);
        }

        await EnsureBoardCacheAsync();

        var items = result.Items.Select(MapFailure).ToList();
        return new PagedResult<AlertItem>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);
    }

    private async Task EnsureBoardCacheAsync()
    {
        if (_boardLocationCache != null)
            return;

        try
        {
            var boards = await _deviceService.GetDevicesAsync(1, 100);
            _boardLocationCache = boards.Items
                .Where(b => Guid.TryParse(b.Id, out _))
                .ToDictionary(b => Guid.Parse(b.Id), b => b.Name);
        }
        catch
        {
            _boardLocationCache = new Dictionary<Guid, string>();
        }
    }

    private AlertItem MapFailure(FailureDto dto)
    {
        var deviceInfo = _boardLocationCache != null && _boardLocationCache.TryGetValue(dto.BoardId, out var loc) && !string.IsNullOrWhiteSpace(loc)
            ? loc
            : dto.BoardMacAddress;

        var date = DateTime.TryParse(dto.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.ToLocalTime()
            : DateTime.Now;

        return new AlertItem
        {
            Id = dto.Id.ToString(),
            BoardId = dto.BoardId,
            Title = dto.FailureCause,
            DeviceInfo = deviceInfo,
            Severity = AlertSeverity.Critical,
            Date = date,
            IsResolved = false
        };
    }
}
