using System;

namespace Venther.App.Models;

public record CheckoutQueueItem(string WorkOrderId, DateTime CompletedAtLocal);
