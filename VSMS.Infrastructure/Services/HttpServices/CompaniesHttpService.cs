using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Domain.Models;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class CompaniesHttpService(
    ILogger<CompaniesHttpService> logger,
    ILocalStorageService localStorage,
    IApplicationSettings applicationSettings) : BaseHttpService(localStorage, applicationSettings, "Companies")
{
    public async Task<CompanyViewModel?> GetCompanyById(Guid companyId)
    {
        try
        {
            var result = await Client.GetAsync(Url($"{companyId}"));
            var content = await result.Content.ReadFromJsonAsync<CompanyViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<List<UserProfileViewModel>?> GetAllUsersInCompany(Guid companyId)
    {
        try
        {
            var result = await Client.GetAsync(Url($"{companyId}/users"));
            if (!result.IsSuccessStatusCode)
                return null;
            
            var content = await result.Content.ReadFromJsonAsync<List<UserProfileViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<CompanyViewModel?> CreateCompany(CompanyViewModel model)
    {
        try
        {
            var result = await Client.PostAsJsonAsync(Url(), model);
            var content = await result.Content.ReadFromJsonAsync<CompanyViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<CompanyViewModel?> UpdateCompany(CompanyViewModel model)
    {
        try
        {
            var result = await Client.PutAsJsonAsync(Url(), model);
            var content = await result.Content.ReadFromJsonAsync<CompanyViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<bool> DeleteCompany(Guid companyId)
    {
        try
        {
            var result = await Client.DeleteAsync(Url($"{companyId}"));
            var content = result.IsSuccessStatusCode;
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
    public async Task<bool> AssignUserToCompany(Guid companyId, Guid userId)
    {
        try
        {
            var result = await Client.PostAsync(Url($"{companyId}/users/{userId}"), null);
            var content = result.IsSuccessStatusCode && await result.Content.ReadFromJsonAsync<bool>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
    
    public async Task<bool> UnassignUserToCompany(Guid companyId, Guid userId)
    {
        try
        {
            var result = await Client.DeleteAsync(Url($"{companyId}/users/{userId}"));
            var content = result.IsSuccessStatusCode && await result.Content.ReadFromJsonAsync<bool>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}