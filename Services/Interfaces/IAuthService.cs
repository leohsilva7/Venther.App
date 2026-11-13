using System.Threading.Tasks;

namespace Venther.App.Services.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> SendPasswordResetEmailAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> IsAuthenticatedAsync();
}
