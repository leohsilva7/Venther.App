using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services.Mocks;

public class MockAlertService : IAlertService
{
    private readonly List<AlertItem> _alerts = new()
    {
        new AlertItem
        {
            Id = "ALT-001",
            Title = "Filtro saturado detectado",
            DeviceInfo = "Self Contained - Sala de Servidores",
            Severity = AlertSeverity.Warning,
            Date = DateTime.Now.AddHours(-2),
            IsResolved = false
        },
        new AlertItem
        {
            Id = "ALT-002",
            Title = "Pressão alta de descarga",
            DeviceInfo = "VRF Condensadora 02",
            Severity = AlertSeverity.Critical,
            Date = DateTime.Now.AddHours(-1),
            IsResolved = false
        },
        new AlertItem
        {
            Id = "ALT-003",
            Title = "Variação térmica além do limite",
            DeviceInfo = "Chiller Principal - Ala Sul",
            Severity = AlertSeverity.Warning,
            Date = DateTime.Now.AddDays(-1),
            IsResolved = true
        }
    };

    public Task<IReadOnlyList<AlertItem>> GetAlertsAsync()
    {
        return Task.FromResult<IReadOnlyList<AlertItem>>(_alerts.AsReadOnly());
    }

    public Task<AlertItem?> GetAlertByIdAsync(string id)
    {
        var alert = _alerts.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(alert);
    }

    public Task<bool> ResolveAlertAsync(string id)
    {
        var alert = _alerts.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
        if (alert == null)
            return Task.FromResult(false);

        alert.IsResolved = true;
        return Task.FromResult(true);
    }
}
