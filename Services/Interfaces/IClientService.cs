using System;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public interface IClientService
{
    Task<CachedResult<PagedResult<CustomerDto>>> GetCustomersAsync(int page = 1, int pageSize = 100);
    Task<CachedResult<CustomerDetailDto?>> GetCustomerByIdAsync(Guid id);
}
