using System;

namespace Venther.App.Models;

public enum WorkOrderStatus
{
    /// <summary>Rotina agendada — card secundário.</summary>
    Scheduled,
    /// <summary>Em andamento / técnico despachado — card com barra animada.</summary>
    InProgress,
    /// <summary>Concluída.</summary>
    Done
}

public class WorkOrder
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsDone { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Scheduled;

    // Variante InProgress
    public string? TechnicianName { get; set; }
    public string? TelemetrySnippet { get; set; }   // ex: "Temp. 28°C · Pressão 12 bar"

    // Variante Scheduled
    public string? ScheduledDateLabel { get; set; } // ex: "Hoje" / "Amanhã" / "15/11/2026"
}
