using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public record FaultRaisedDto(Guid BoardId, string FailureCause, string Date);

public interface ITelemetryHubService
{
    bool IsConnected { get; }
    event Action<BoardTelemetryDto>? TelemetryUpdated;
    event Action<FaultRaisedDto>? FaultRaised;
    event Action<bool>? ConnectionStateChanged;

    Task ConnectAsync();
    Task DisconnectAsync();
    Task<IReadOnlyList<BoardTelemetryDto>> SubscribeAsync(IEnumerable<Guid> boardIds);
    Task UnsubscribeAsync(IEnumerable<Guid> boardIds);
    BoardTelemetryDto? GetLatestTelemetry(Guid boardId);
    IReadOnlyList<double> GetTrendBuffer(Guid boardId);
}
