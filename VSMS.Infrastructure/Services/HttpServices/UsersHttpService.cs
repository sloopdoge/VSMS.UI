using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class UsersHttpService(
    ILogger<CompaniesHttpService> logger,
    ILocalStorageService localStorage,
    IApplicationSettings applicationSettings) : BaseHttpService(localStorage, applicationSettings, "Users")
{
    public async Task<List<UserProfileViewModel>?> GetAllUserProfiles()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url());
            var content = await result.Content.ReadFromJsonAsync<List<UserProfileViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<UserProfileViewModel?> GetUserProfileById(Guid userId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"{userId}"));
            var content = await result.Content.ReadFromJsonAsync<UserProfileViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<UserProfileViewModel?> CreateUser(UserCreateViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.PostAsJsonAsync(Url(), model);
            var content = await result.Content.ReadFromJsonAsync<UserProfileViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<UserProfileViewModel?> UpdateUser(UserProfileViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.PutAsJsonAsync(Url($"{model.Id}"), model);
            var content = await result.Content.ReadFromJsonAsync<UserProfileViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<bool> DeleteUser(Guid userId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.DeleteAsync(Url($"{userId}"));
            return result.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}