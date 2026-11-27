using System;
using System.Text.Json.Serialization;

namespace Venther.App.Services.Api.Dtos;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? PhoneNumber,
    DateTime? CreatedAt
);

public record LoginResponseDto(
    string Token,
    int ExpiresIn,
    UserDto User
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record ForgotPasswordRequestDto(
    string Email
);

public record ForgotPasswordResponseDto(
    string Message
);

public record ResetPasswordRequestDto(
    string Email,
    string Code,
    string Password,
    string ConfirmPassword
);

public record ResetPasswordResponseDto(
    string Message
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string Location,
    string PhoneNumber,
    DateTime? CreatedAt
);

public record CustomerBoardDto(
    Guid Id,
    string MacAddress,
    string Location,
    string InstallationDate
);

public record CustomerDetailDto(
    Guid Id,
    string Name,
    string Email,
    string Location,
    string PhoneNumber,
    DateTime? CreatedAt,
    IReadOnlyList<CustomerBoardDto> Boards
);

public record BoardDto(
    Guid Id,
    string MacAddress,
    Guid CustomerId,
    string CustomerName,
    string InstallationDate,
    string Location,
    DateTime? CreatedAt
);

public record CreateBoardRequestDto(
    string MacAddress,
    Guid CustomerId,
    string InstallationDate,
    string Location
);

public record BoardLogDto(
    Guid Id,
    Guid BoardId,
    double SupplyAirTemperature,
    double ReturnAirTemperature,
    double CondensationAirTemperature,
    double SpaceTemperature,
    double GasLineTemperature,
    double LiquidLineTemperature,
    double EvaporatorCoilPressureDrop,
    double CondenserCoilPressureDrop,
    DateTime? CreatedAt
);

public record FailureDto(
    int Id,
    Guid BoardId,
    string BoardMacAddress,
    string Date,
    string FailureCause,
    string? ReportId,
    int? MaintenanceRequestId
);

public record MaintenanceRequestDto(
    int Id,
    Guid BoardId,
    string BoardMacAddress,
    string BoardLocation,
    Guid UserId,
    string UserName,
    string UserEmail,
    string Date,
    bool IsDone
);

public record CompleteMaintenanceRequestDto(
    bool IsDone
);

public record BoardTelemetryDto(
    Guid BoardId,
    string MacAddress,
    double SupplyAirTemperature,
    double ReturnAirTemperature,
    double CondensationAirTemperature,
    double SpaceTemperature,
    double GasLineTemperature,
    double LiquidLineTemperature,
    double EvaporatorCoilPressureDrop,
    double CondenserCoilPressureDrop,
    bool IsOnline,
    [property: JsonPropertyName("receivedAt")]
    DateTime? LastSeen
);
