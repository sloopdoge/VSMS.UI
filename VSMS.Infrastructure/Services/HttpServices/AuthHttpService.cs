using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Domain.Models;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class AuthHttpService(
    ILogger<AuthHttpService> logger,
    ILocalStorageService localStorage,
    IApplicationSettings applicationSettings) : BaseHttpService(localStorage, applicationSettings, "Auth")
{
    public async Task<LoginResultModel?> Login(UserLoginViewModel model)
    {
        try
        {
            var result = await Client.PostAsJsonAsync(Url($"Login"), model);
            var content = await result.Content.ReadFromJsonAsync<LoginResultModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<UserRegisterResultModel?> Register(UserRegisterViewModel model)
    {
        try
        {
            var result = await Client.PostAsJsonAsync(Url($"Register"), model);
            var content = await result.Content.ReadFromJsonAsync<UserRegisterResultModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<TokenValidationResultModel?> ValidateToken()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"Token/Validate"));
            var content = await result.Content.ReadFromJsonAsync<TokenValidationResultModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
}