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
    private readonly ICacheService _cacheService;
    private Dictionary<Guid, string>? _boardLocationCache;

    public AlertService(IApiClient apiClient, IDeviceService deviceService, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _deviceService = deviceService;
        _cacheService = cacheService;
    }

    public async Task<CachedResult<PagedResult<AlertItem>>> GetAlertsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var result = await _apiClient.GetAsync<PagedResult<FailureDto>>($"/failures?page={page}&pageSize={pageSize}");
            if (result == null)
            {
                var empty = new PagedResult<AlertItem>(Array.Empty<AlertItem>(), page, pageSize, 0, 0);
                return CachedResult<PagedResult<AlertItem>>.Fresh(empty);
            }

            await EnsureBoardCacheAsync();

            var items = result.Items.Select(MapFailure).ToList();
            var paged = new PagedResult<AlertItem>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);

            if (page == 1)
            {
                await _cacheService.SaveAsync("alerts", paged);
            }

            return CachedResult<PagedResult<AlertItem>>.Fresh(paged);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<PagedResult<AlertItem>>("alerts");
            if (cached != null)
            {
                return CachedResult<PagedResult<AlertItem>>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    private async Task EnsureBoardCacheAsync()
    {
        if (_boardLocationCache != null)
            return;

        try
        {
            var boardsResult = await _deviceService.GetDevicesAsync(1, 100);
            _boardLocationCache = boardsResult.Data.Items
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
