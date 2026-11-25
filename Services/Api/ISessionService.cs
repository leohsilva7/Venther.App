using System.Threading.Tasks;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Api;

public interface ISessionService
{
    bool IsAuthenticated { get; }
    string? Token { get; }
    UserDto? CurrentUser { get; }

    Task RestoreAsync();
    Task SaveAsync(string token, int expiresInSeconds, UserDto user);
    Task ClearAsync();
}
