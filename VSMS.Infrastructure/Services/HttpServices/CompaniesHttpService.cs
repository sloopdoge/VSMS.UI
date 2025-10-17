using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Domain.Models;
using VSMS.Domain.Models.ViewModels;
using VSMS.Domain.Models.ViewModels.Shared.Filters;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class CompaniesHttpService(
    ILogger<CompaniesHttpService> logger,
    ILocalStorageService localStorage,
    IApplicationSettings applicationSettings) : BaseHttpService(localStorage, applicationSettings, "Companies")
{
    /// <summary>
    /// Retrieves a single company by its identifier.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <returns>The company view model if found; otherwise, <c>null</c>.</returns>
    public async Task<CompanyViewModel?> GetCompanyById(Guid companyId)
    {
        try
        {
            await AddAuthorizationAsync();
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
    
    /// <summary>
    /// Gets the collection of all available companies from the API.
    /// </summary>
    /// <returns>A list of companies or <c>null</c> if the request fails.</returns>
    public async Task<List<CompanyViewModel>?> GetAllCompanies()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url());
            var content = await result.Content.ReadFromJsonAsync<List<CompanyViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Gets the collection of all available companies from the API.
    /// </summary>
    /// <returns>A list of companies or <c>null</c> if the request fails.</returns>
    public async Task<PagedResult<CompanyViewModel>?> GetCompaniesByFilter(CompaniesFilterViewModel filter)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.PostAsJsonAsync(Url("ByFilter"), filter);
            var content = await result.Content.ReadFromJsonAsync<PagedResult<CompanyViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Retrieves all users assigned to the specified company.
    /// </summary>
    /// <param name="companyId">The identifier of the company.</param>
    /// <returns>List of user profiles or <c>null</c> if the request fails.</returns>
    public async Task<List<UserProfileViewModel>?> GetAllUsersInCompany(Guid companyId)
    {
        try
        {
            await AddAuthorizationAsync();
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
    
    /// <summary>
    /// Creates a new company using the provided view model.
    /// </summary>
    /// <param name="model">The company details to create.</param>
    /// <returns>The created company view model or <c>null</c> on failure.</returns>
    public async Task<CompanyViewModel?> CreateCompany(CompanyViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
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
    
    /// <summary>
    /// Updates an existing company with the supplied information.
    /// </summary>
    /// <param name="model">The company data to update.</param>
    /// <returns>The updated company view model or <c>null</c> on failure.</returns>
    public async Task<CompanyViewModel?> UpdateCompany(CompanyViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
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
    
    /// <summary>
    /// Deletes a company by its identifier.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <returns><c>true</c> if deletion succeeds; otherwise, <c>false</c>.</returns>
    public async Task<bool> DeleteCompanyById(Guid companyId)
    {
        try
        {
            await AddAuthorizationAsync();
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
    /// <summary>
    /// Assigns a user to the specified company.
    /// </summary>
    /// <param name="companyId">The target company identifier.</param>
    /// <param name="userId">The identifier of the user to assign.</param>
    /// <returns><c>true</c> if the assignment succeeds; otherwise, <c>false</c>.</returns>
    public async Task<bool> AssignUserToCompany(Guid companyId, Guid userId)
    {
        try
        {
            await AddAuthorizationAsync();
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
    
    /// <summary>
    /// Removes the association between a user and a company.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if unassignment succeeds; otherwise, <c>false</c>.</returns>
    public async Task<bool> UnassignUserToCompany(Guid companyId, Guid userId)
    {
        try
        {
            await AddAuthorizationAsync();
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