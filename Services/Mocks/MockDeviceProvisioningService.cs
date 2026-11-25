using System.Threading.Tasks;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services.Mocks;

public class MockDeviceProvisioningService : IDeviceProvisioningService
{
    public async Task<bool> ProvisionDeviceAsync(string hardwareId, string ssid, string password)
    {
        // Simula o teste/provisionamento com atraso de 2 segundos
        await Task.Delay(2000);
        return true;
    }
}
