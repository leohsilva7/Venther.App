using System;
using System.Threading.Tasks;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public interface IClientService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 100);
    Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id);
}
