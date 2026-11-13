using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Models;

namespace Venther.App.Services.Interfaces;

public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrder>> GetWorkOrdersAsync();
    Task<WorkOrder?> GetWorkOrderByIdAsync(string id);
    Task<bool> CreateWorkOrderAsync(WorkOrder workOrder);
    Task<bool> CompleteWorkOrderAsync(string id);
}
