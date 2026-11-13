using System;

namespace Venther.App.Models;

public class AlertItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsResolved { get; set; }
}
