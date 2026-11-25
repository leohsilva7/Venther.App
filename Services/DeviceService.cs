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

    public DeviceService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<PagedResult<Device>> GetDevicesAsync(int page = 1, int pageSize = 20)
    {
        var result = await _apiClient.GetAsync<PagedResult<BoardDto>>($"/boards?page={page}&pageSize={pageSize}");
        if (result == null)
        {
            return new PagedResult<Device>(Array.Empty<Device>(), page, pageSize, 0, 0);
        }

        var items = result.Items.Select(MapBoard).ToList();
        return new PagedResult<Device>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);
    }

    public async Task<Device?> GetDeviceByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var boardId))
            return null;

        var board = await _apiClient.GetAsync<BoardDto>($"/boards/{boardId}");
        return board == null ? null : MapBoard(board);
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

        return MapBoard(created);
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
