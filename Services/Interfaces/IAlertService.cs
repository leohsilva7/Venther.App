using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Models;

namespace Venther.App.Services.Interfaces;

public interface IAlertService
{
    Task<IReadOnlyList<AlertItem>> GetAlertsAsync();
    Task<AlertItem?> GetAlertByIdAsync(string id);
    Task<bool> ResolveAlertAsync(string id);
}
