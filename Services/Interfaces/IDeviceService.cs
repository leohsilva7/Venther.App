using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api.Dtos;
using Device = Venther.App.Models.Device;

namespace Venther.App.Services.Interfaces;

public interface IDeviceService
{
    Task<CachedResult<PagedResult<Device>>> GetDevicesAsync(int page = 1, int pageSize = 20);
    Task<CachedResult<Device?>> GetDeviceByIdAsync(string id);
    Task<Device> RegisterDeviceAsync(Guid customerId, string macAddress, string location);
    Task<bool> UpdateDeviceStatusAsync(string id, DeviceStatus status);
    Task<IReadOnlyList<BoardLogDto>> GetBoardLogsAsync(Guid boardId, string dateMonth);
}
