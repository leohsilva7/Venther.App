using System;
using System.Threading.Tasks;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class ClientService : IClientService
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;

    public ClientService(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<CachedResult<PagedResult<CustomerDto>>> GetCustomersAsync(int page = 1, int pageSize = 100)
    {
        try
        {
            var result = await _apiClient.GetAsync<PagedResult<CustomerDto>>($"/customers?page={page}&pageSize={pageSize}");
            var paged = result ?? new PagedResult<CustomerDto>(Array.Empty<CustomerDto>(), page, pageSize, 0, 0);

            if (page == 1)
            {
                await _cacheService.SaveAsync("customers", paged);
            }

            return CachedResult<PagedResult<CustomerDto>>.Fresh(paged);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<PagedResult<CustomerDto>>("customers");
            if (cached != null)
            {
                return CachedResult<PagedResult<CustomerDto>>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }

    public async Task<CachedResult<CustomerDetailDto?>> GetCustomerByIdAsync(Guid id)
    {
        var cacheKey = $"customers:{id}";

        try
        {
            var detail = await _apiClient.GetAsync<CustomerDetailDto>($"/customers/{id}");
            if (detail != null)
            {
                await _cacheService.SaveAsync(cacheKey, detail);
            }
            return CachedResult<CustomerDetailDto?>.Fresh(detail);
        }
        catch (ApiException ex) when (ex.StatusCode == 0)
        {
            var cached = await _cacheService.LoadAsync<CustomerDetailDto>(cacheKey);
            if (cached != null)
            {
                return CachedResult<CustomerDetailDto?>.FromStorage(cached.Data, cached.SavedAt);
            }
            throw;
        }
    }
}
