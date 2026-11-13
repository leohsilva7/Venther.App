using System.Threading.Tasks;
using Venther.App.Services.Interfaces;

namespace Venther.App.Services.Mocks;

public class MockAuthService : IAuthService
{
    private bool _isAuthenticated;

    public Task<bool> LoginAsync(string email, string password)
    {
        _isAuthenticated = !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
        return Task.FromResult(_isAuthenticated);
    }

    public Task LogoutAsync()
    {
        _isAuthenticated = false;
        return Task.CompletedTask;
    }

    public Task<bool> SendPasswordResetEmailAsync(string email)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(email));
    }

    public Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(newPassword));
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_isAuthenticated);
    }
}
