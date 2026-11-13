using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Models;
using Device = Venther.App.Models.Device;

namespace Venther.App.Services.Interfaces;

public interface IDeviceService
{
    Task<IReadOnlyList<Device>> GetDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(string id);
    Task<bool> RegisterDeviceAsync(Device device);
    Task<bool> UpdateDeviceStatusAsync(string id, DeviceStatus status);
}
