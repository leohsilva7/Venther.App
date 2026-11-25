using System.Threading.Tasks;
using Venther.App.Services.Api.Dtos;

namespace Venther.App.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string code, string password, string confirmPassword);
    bool IsAuthenticated { get; }
    UserDto? CurrentUser { get; }
}
