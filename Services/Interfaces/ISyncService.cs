using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Models;

namespace Venther.App.Services.Interfaces;

public interface ISyncService
{
    Task EnqueueCheckoutAsync(string workOrderId);
    Task<IReadOnlyList<CheckoutQueueItem>> GetQueueAsync();
    Task<bool> IsWorkOrderPendingSyncAsync(string workOrderId);
    Task ProcessQueueAsync();
    Task ClearQueueAsync();
}
