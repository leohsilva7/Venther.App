using System.Threading.Tasks;

namespace Venther.App.Services.Interfaces;

public interface IDeviceProvisioningService
{
    Task<bool> ProvisionDeviceAsync(string hardwareId, string ssid, string password);
}
