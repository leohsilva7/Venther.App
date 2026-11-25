using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Venther.App.Services.Api;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ISessionService _sessionService;

    public AuthHeaderHandler(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _sessionService.Token;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        bool isLoginRoute = requestPath.Contains("/auth/login", System.StringComparison.OrdinalIgnoreCase);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token) && !isLoginRoute)
        {
            await _sessionService.ClearAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//login");
                }
            });
        }

        return response;
    }
}
