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
    /// <summary>
    /// Sends user login credentials to the API and returns a login result.
    /// </summary>
    /// <param name="model">The credentials entered by the user.</param>
    /// <returns>The login result if the request succeeds; otherwise, <c>null</c>.</returns>
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
    
    /// <summary>
    /// Registers a new user with the API using the provided registration model.
    /// </summary>
    /// <param name="model">The registration details for the new user.</param>
    /// <returns>The registration result if the request succeeds; otherwise, <c>null</c>.</returns>
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
    
    /// <summary>
    /// Validates the current authorization token against the API.
    /// </summary>
    /// <returns>The token validation result if the request succeeds; otherwise, <c>null</c>.</returns>
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