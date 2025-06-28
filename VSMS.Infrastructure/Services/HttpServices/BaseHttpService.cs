using Blazored.LocalStorage;
using VSMS.Domain.Constants;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class BaseHttpService
{
    protected readonly HttpClient Client;
    protected readonly ILocalStorageService LocalStorage;
    private readonly string _apiUrl;

    protected BaseHttpService(
        ILocalStorageService localStorage,
        IApplicationSettings applicationSettings,
        string apiUrl)
    {
        LocalStorage = localStorage;
        _apiUrl = apiUrl;

        Client = new();
        Client.BaseAddress = new(applicationSettings.ApiUrl);
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.Timeout = TimeSpan.FromMinutes(1);
        Client.DefaultRequestHeaders.Accept.Add(new("application/json"));
    }
    
    protected async Task AddAuthorizationAsync()
    {
        var token = await GetAuthToken();
            
        if (string.IsNullOrEmpty(token))
            return;          

        Client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    protected string Url(string apiPath = "")
    {
        return string.IsNullOrEmpty(apiPath) ? $"/api/{_apiUrl}" : $"/api/{_apiUrl}/{apiPath}";
    }

    protected async Task<string?> GetAuthToken()
    {
        return await LocalStorage.GetItemAsync<string>(CookieKeys.AuthToken);
    }
}