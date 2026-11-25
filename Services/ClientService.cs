using System;
using System.Threading.Tasks;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class ClientService : IClientService
{
    private readonly IApiClient _apiClient;

    public ClientService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 100)
    {
        var result = await _apiClient.GetAsync<PagedResult<CustomerDto>>($"/customers?page={page}&pageSize={pageSize}");
        return result ?? new PagedResult<CustomerDto>(Array.Empty<CustomerDto>(), page, pageSize, 0, 0);
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id)
    {
        return await _apiClient.GetAsync<CustomerDetailDto>($"/customers/{id}");
    }
}
