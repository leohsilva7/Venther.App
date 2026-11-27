using Microsoft.Maui.Storage;

namespace Venther.App.Helpers;

public static class ApiConfig
{
    private const string BaseUrlPreferenceKey = "api_base_url";

#if DEBUG
    private const string DefaultBaseUrl = "http://10.111.72.9:5084";
#else
    // TODO: definir URL de produção
    private const string DefaultBaseUrl = "https://api.venther.com";
#endif

    public const string ApiPath = "/api";

    public static string BaseUrl
    {
        get
        {
            var saved = Preferences.Default.Get(BaseUrlPreferenceKey, string.Empty);
            return string.IsNullOrWhiteSpace(saved) ? DefaultBaseUrl : saved;
        }
    }

    public static void SetBaseUrl(string url)
    {
        var cleanUrl = url.Trim().TrimEnd('/');
        if (cleanUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            cleanUrl = cleanUrl[..^4];
        }
        Preferences.Default.Set(BaseUrlPreferenceKey, cleanUrl);
    }
}

