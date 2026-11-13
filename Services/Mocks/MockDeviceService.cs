using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Interfaces;
using Device = Venther.App.Models.Device;

namespace Venther.App.Services.Mocks;

public class MockDeviceService : IDeviceService
{
    private readonly List<Device> _devices = new()
    {
        new Device
        {
            Id = "DEV-001",
            Name = "Chiller Principal - Ala Sul",
            Location = "Ed. Corporate Tower - 4º Andar",
            Status = DeviceStatus.Normal,
            LastReading = "7.2°C • Operação normal"
        },
        new Device
        {
            Id = "DEV-002",
            Name = "Fancoil Auditório Principal",
            Location = "Térreo",
            Status = DeviceStatus.Normal,
            LastReading = "21.0°C • Estável"
        },
        new Device
        {
            Id = "DEV-003",
            Name = "Self Contained - Sala de Servidores",
            Location = "Subsolo",
            Status = DeviceStatus.Attention,
            LastReading = "Filtro saturado detectado"
        },
        new Device
        {
            Id = "DEV-004",
            Name = "VRF Condensadora 02",
            Location = "Cobertura - Bloco B",
            Status = DeviceStatus.Critical,
            LastReading = "Pressão alta de descarga"
        }
    };

    public Task<IReadOnlyList<Device>> GetDevicesAsync()
    {
        return Task.FromResult<IReadOnlyList<Device>>(_devices.AsReadOnly());
    }

    public Task<Device?> GetDeviceByIdAsync(string id)
    {
        var device = _devices.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(device);
    }

    public Task<bool> RegisterDeviceAsync(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Id))
        {
            device.Id = $"DEV-{_devices.Count + 1:D3}";
        }
        _devices.Add(device);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateDeviceStatusAsync(string id, DeviceStatus status)
    {
        var device = _devices.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        if (device == null)
            return Task.FromResult(false);

        device.Status = status;
        return Task.FromResult(true);
    }
}
