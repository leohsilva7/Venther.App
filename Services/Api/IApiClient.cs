using System.Threading;
using System.Threading.Tasks;

namespace Venther.App.Services.Api;

public interface IApiClient
{
    Task<TResult?> GetAsync<TResult>(string endpoint, CancellationToken cancellationToken = default);
    Task<TResult?> PostAsync<TBody, TResult>(string endpoint, TBody body, CancellationToken cancellationToken = default);
    Task PostAsync<TBody>(string endpoint, TBody body, CancellationToken cancellationToken = default);
    Task PutAsync<TBody>(string endpoint, TBody body, CancellationToken cancellationToken = default);
    Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}
