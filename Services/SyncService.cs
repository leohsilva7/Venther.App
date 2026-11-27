using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Api.Dtos;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services;

public class SyncService : ISyncService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SyncService>? _logger;
    private readonly string _queueFilePath;
    private static readonly SemaphoreSlim QueueLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private int _isProcessing = 0;

    public SyncService(IApiClient apiClient, ILogger<SyncService>? logger = null)
    {
        _apiClient = apiClient;
        _logger = logger;
        _queueFilePath = Path.Combine(FileSystem.AppDataDirectory, "checkout_queue.json");

        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            _ = ProcessQueueAsync();
        }
    }

    private async Task<List<CheckoutQueueItem>> LoadQueueInternalAsync()
    {
        if (!File.Exists(_queueFilePath))
            return new List<CheckoutQueueItem>();

        try
        {
            var json = await File.ReadAllTextAsync(_queueFilePath);
            return JsonSerializer.Deserialize<List<CheckoutQueueItem>>(json, JsonOptions) ?? new List<CheckoutQueueItem>();
        }
        catch
        {
            return new List<CheckoutQueueItem>();
        }
    }

    private async Task SaveQueueInternalAsync(List<CheckoutQueueItem> items)
    {
        var temp = $"{_queueFilePath}.{Guid.NewGuid():N}.tmp";
        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(temp, json);
        if (File.Exists(_queueFilePath))
        {
            File.Delete(_queueFilePath);
        }
        File.Move(temp, _queueFilePath);
    }

    public async Task EnqueueCheckoutAsync(string workOrderId)
    {
        await QueueLock.WaitAsync();
        try
        {
            var queue = await LoadQueueInternalAsync();
            if (!queue.Any(x => x.WorkOrderId == workOrderId))
            {
                queue.Add(new CheckoutQueueItem(workOrderId, DateTime.Now));
                await SaveQueueInternalAsync(queue);
            }
        }
        finally
        {
            QueueLock.Release();
        }
    }

    public async Task<IReadOnlyList<CheckoutQueueItem>> GetQueueAsync()
    {
        await QueueLock.WaitAsync();
        try
        {
            return await LoadQueueInternalAsync();
        }
        finally
        {
            QueueLock.Release();
        }
    }

    public async Task<bool> IsWorkOrderPendingSyncAsync(string workOrderId)
    {
        await QueueLock.WaitAsync();
        try
        {
            var queue = await LoadQueueInternalAsync();
            return queue.Any(x => x.WorkOrderId == workOrderId);
        }
        finally
        {
            QueueLock.Release();
        }
    }

    public async Task ProcessQueueAsync()
    {
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return;

        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return;

            await QueueLock.WaitAsync();
            List<CheckoutQueueItem> queue;
            try
            {
                queue = await LoadQueueInternalAsync();
            }
            finally
            {
                QueueLock.Release();
            }

            if (queue.Count == 0)
                return;

            var remaining = new List<CheckoutQueueItem>();

            foreach (var item in queue)
            {
                if (!int.TryParse(item.WorkOrderId, out var requestId))
                {
                    _logger?.LogWarning("Removendo item de checkout com ID inválido: {WorkOrderId}", item.WorkOrderId);
                    continue;
                }

                try
                {
                    await _apiClient.PutAsync($"/maintence_requests/{requestId}", new CompleteMaintenanceRequestDto(true));
                    _logger?.LogInformation("Checkout da O.S. #{WorkOrderId} sincronizado com sucesso.", item.WorkOrderId);
                }
                catch (ApiException ex) when (ex.StatusCode is 400 or 404)
                {
                    _logger?.LogError(ex, "Removendo O.S. #{WorkOrderId} da fila devido a erro permanente HTTP {StatusCode}", item.WorkOrderId, ex.StatusCode);
                }
                catch (ApiException ex) when (ex.StatusCode == 0)
                {
                    // Erro de conexão/rede: mantém o item na fila
                    remaining.Add(item);
                }
                catch (Exception ex)
                {
                    // Outro erro de rede/servidor inesperado: mantém para tentar depois
                    _logger?.LogWarning(ex, "Falha temporária ao sincronizar O.S. #{WorkOrderId}. Mantendo na fila.", item.WorkOrderId);
                    remaining.Add(item);
                }
            }

            await QueueLock.WaitAsync();
            try
            {
                await SaveQueueInternalAsync(remaining);
            }
            finally
            {
                QueueLock.Release();
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isProcessing, 0);
        }
    }

    public async Task ClearQueueAsync()
    {
        await QueueLock.WaitAsync();
        try
        {
            if (File.Exists(_queueFilePath))
            {
                try { File.Delete(_queueFilePath); } catch { }
            }
        }
        finally
        {
            QueueLock.Release();
        }
    }
}
