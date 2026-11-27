using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Venther.App.Helpers;

namespace Venther.App.Services.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    private static string BuildUrl(string endpoint)
    {
        var baseUrl = ApiConfig.BaseUrl.Trim().TrimEnd('/');
        var cleanEndpoint = endpoint.Trim().TrimStart('/');

        // Evita duplicar /api se a BaseUrl já tiver /api ou se o endpoint já começar com api/
        if (cleanEndpoint.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            cleanEndpoint = cleanEndpoint[4..];
        }

        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl[..^4];
        }

        return $"{baseUrl}/{ApiConfig.ApiPath.Trim('/')}/{cleanEndpoint}";
    }

    public async Task<TResult?> GetAsync<TResult>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(BuildUrl(endpoint), cancellationToken);
            await EnsureSuccessOrThrowAsync(response, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<TResult>(JsonOptions, cancellationToken);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            throw new ApiException("Não foi possível conectar ao servidor.", 0, ex);
        }
    }

    public async Task<TResult?> PostAsync<TBody, TResult>(string endpoint, TBody body, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), body, JsonOptions, cancellationToken);
            await EnsureSuccessOrThrowAsync(response, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<TResult>(JsonOptions, cancellationToken);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            throw new ApiException("Não foi possível conectar ao servidor.", 0, ex);
        }
    }

    public async Task PostAsync<TBody>(string endpoint, TBody body, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), body, JsonOptions, cancellationToken);
            await EnsureSuccessOrThrowAsync(response, cancellationToken);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            throw new ApiException("Não foi possível conectar ao servidor.", 0, ex);
        }
    }

    public async Task PutAsync<TBody>(string endpoint, TBody body, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(BuildUrl(endpoint), body, JsonOptions, cancellationToken);
            await EnsureSuccessOrThrowAsync(response, cancellationToken);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            throw new ApiException("Não foi possível conectar ao servidor.", 0, ex);
        }
    }

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(BuildUrl(endpoint), cancellationToken);
            await EnsureSuccessOrThrowAsync(response, cancellationToken);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            throw new ApiException("Não foi possível conectar ao servidor.", 0, ex);
        }
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        int statusCode = (int)response.StatusCode;
        string? serverErrorMessage = null;

        try
        {
            var errorDoc = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
            if (errorDoc.ValueKind == JsonValueKind.Object)
            {
                if (errorDoc.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
                {
                    serverErrorMessage = errorProp.GetString();
                }
                else if (errorDoc.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                {
                    serverErrorMessage = msgProp.GetString();
                }
            }
        }
        catch
        {
            // Ignore parse errors from body
        }

        string defaultMessage = statusCode switch
        {
            400 => "Requisição inválida.",
            401 => "Sua sessão expirou. Faça login novamente.",
            403 => "Você não tem permissão para realizar esta ação.",
            404 => "Recurso não encontrado.",
            409 => "Ocorreu um conflito com os dados informados.",
            500 => "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
            _ => "Ocorreu um erro na comunicação com o servidor."
        };

        // Se veio mensagem de erro da API e não for o padrão em inglês de erro interno
        string finalMessage = !string.IsNullOrWhiteSpace(serverErrorMessage)
            ? serverErrorMessage
            : defaultMessage;

        throw new ApiException(finalMessage, statusCode);
    }
}
