using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services.Mocks;

public class MockWorkOrderService : IWorkOrderService
{
    private readonly List<WorkOrder> _workOrders = new()
    {
        // ── Variante InProgress: técnico despachado ──────────────────────────
        new WorkOrder
        {
            Id = "OS-2024-089",
            Title = "Manutenção Corretiva Urgente",
            DeviceName = "Self Contained - Sala de Servidores",
            Location = "Subsolo · Bloco B",
            Date = DateTime.Now,
            IsDone = false,
            Status = WorkOrderStatus.InProgress,
            TechnicianName = "Carlos Mendes",
            TelemetrySnippet = "Temp. 32°C · Pressão 14 bar · Umid. 68%"
        },

        // ── Variante Scheduled: rotina agendada ──────────────────────────────
        new WorkOrder
        {
            Id = "OS-2024-090",
            Title = "Limpeza de Filtros e Serpentinas",
            DeviceName = "Chiller Principal - Ala Sul",
            Location = "Ed. Corporate Tower · 4º Andar",
            Date = DateTime.Today.AddDays(1),
            IsDone = false,
            Status = WorkOrderStatus.Scheduled,
            ScheduledDateLabel = "Amanhã, 14h00"
        },
        new WorkOrder
        {
            Id = "OS-2024-091",
            Title = "Revisão Geral Preventiva",
            DeviceName = "Fancoil Auditório Principal",
            Location = "Térreo · Bloco A",
            Date = DateTime.Today.AddDays(3),
            IsDone = false,
            Status = WorkOrderStatus.Scheduled,
            ScheduledDateLabel = "15/11, 09h00"
        },
        new WorkOrder
        {
            Id = "OS-2024-087",
            Title = "Calibração dos Sensores de Temperatura",
            DeviceName = "VRF Condensadora 02",
            Location = "Cobertura · Bloco C",
            Date = DateTime.Today.AddDays(-3),
            IsDone = true,
            Status = WorkOrderStatus.Done,
            ScheduledDateLabel = "10/11/2026"
        },
        new WorkOrder
        {
            Id = "OS-2024-088",
            Title = "Inspeção do Compressor e Fluido Refrigerante",
            DeviceName = "VRF Condensadora 02",
            Location = "Cobertura · Bloco C",
            Date = DateTime.Today.AddDays(-1),
            IsDone = true,
            Status = WorkOrderStatus.Done,
            ScheduledDateLabel = "12/11/2026"
        },
    };

    public Task<IReadOnlyList<WorkOrder>> GetWorkOrdersAsync()
        => Task.FromResult<IReadOnlyList<WorkOrder>>(_workOrders.AsReadOnly());

    public Task<WorkOrder?> GetWorkOrderByIdAsync(string id)
    {
        var item = _workOrders.FirstOrDefault(w => string.Equals(w.Id, id, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(item);
    }

    public Task<bool> CreateWorkOrderAsync(WorkOrder workOrder)
    {
        if (string.IsNullOrWhiteSpace(workOrder.Id))
            workOrder.Id = $"OS-{DateTime.Now:yyyyMMdd-HHmm}";
        _workOrders.Add(workOrder);
        return Task.FromResult(true);
    }

    public Task<bool> CompleteWorkOrderAsync(string id)
    {
        var item = _workOrders.FirstOrDefault(w => string.Equals(w.Id, id, StringComparison.OrdinalIgnoreCase));
        if (item == null) return Task.FromResult(false);
        item.IsDone = true;
        item.Status = WorkOrderStatus.Done;
        return Task.FromResult(true);
    }
}
