using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;
using Device = Venther.App.Models.Device;

namespace Venther.App.Services;

public class DeviceService : IDeviceService
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;

    public DeviceService(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<CachedResult<PagedResult<Device>>> GetDevicesAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var result = await _apiClient.GetAsync<PagedResult<BoardDto>>($"/boards?page={page}&pageSize={pageSize}");
            if (result == null)
            {
                var empty = new PagedResult<Device>(Array.Empty<Device>(), page, pageSize, 0, 0);
                return CachedResult<PagedResult<Device>>.Fresh(empty);
            }

            var items = result.Items.Select(MapBoard).ToList();
            var paged = new PagedResult<Device>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);

            if (page == 1)
            {
                await _cacheService.SaveAsync("devices", paged);
            }

            return CachedResult<PagedResult<Device>>.Fresh(paged);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<PagedResult<Device>>("devices");
            if (cached != null)
            {
                return CachedResult<PagedResult<Device>>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    public async Task<CachedResult<Device?>> GetDeviceByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var boardId))
            return CachedResult<Device?>.Fresh(null);

        var cacheKey = $"board:{boardId}";

        try
        {
            var board = await _apiClient.GetAsync<BoardDto>($"/boards/{boardId}");
            var device = board == null ? null : MapBoard(board);
            if (device != null)
            {
                await _cacheService.SaveAsync(cacheKey, device);
            }
            return CachedResult<Device?>.Fresh(device);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<Device>(cacheKey);
            if (cached != null)
            {
                return CachedResult<Device?>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    public async Task<Device> RegisterDeviceAsync(Guid customerId, string macAddress, string location)
    {
        var request = new CreateBoardRequestDto(
            MacAddress: macAddress.Trim().ToUpperInvariant(),
            CustomerId: customerId,
            InstallationDate: DateTime.Today.ToString("yyyy-MM-dd"),
            Location: location.Trim());

        var created = await _apiClient.PostAsync<CreateBoardRequestDto, BoardDto>("/boards", request);
        if (created == null)
        {
            throw new ApiException("Falha ao processar resposta do servidor.", 500);
        }

        var device = MapBoard(created);
        await _cacheService.SaveAsync($"board:{device.Id}", device);
        return device;
    }

    public Task<bool> UpdateDeviceStatusAsync(string id, DeviceStatus status)
    {
        return Task.FromResult(true);
    }

    public async Task<IReadOnlyList<BoardLogDto>> GetBoardLogsAsync(Guid boardId, string dateMonth)
    {
        var result = await _apiClient.GetAsync<IReadOnlyList<BoardLogDto>>($"/boards/{boardId}/logs?date={dateMonth}");
        return result ?? Array.Empty<BoardLogDto>();
    }

    private static Device MapBoard(BoardDto board) => new()
    {
        Id = board.Id.ToString(),
        Name = board.Location,
        Location = board.CustomerName,
        HardwareId = board.MacAddress,
        CustomerId = board.CustomerId,
        InstallationDate = board.InstallationDate,
        Status = DeviceStatus.Attention,
        LastReading = "Sem leitura"
    };
}
