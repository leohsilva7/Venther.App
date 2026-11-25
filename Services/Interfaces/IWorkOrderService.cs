using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrder>> GetWorkOrdersAsync(int page = 1, int pageSize = 20);
    Task<WorkOrder?> GetWorkOrderByIdAsync(string id);
    Task<bool> CompleteWorkOrderAsync(string id);
}
