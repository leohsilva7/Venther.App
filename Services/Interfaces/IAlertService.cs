using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public interface IAlertService
{
    Task<CachedResult<PagedResult<AlertItem>>> GetAlertsAsync(int page = 1, int pageSize = 20);
}
