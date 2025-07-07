using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using VSMS.Application.Identity;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Services.HttpServices;
using static VSMS.Domain.Constants.RoleNames;

namespace VSMS.Application.Components.Pages.Companies;

public partial class Company : ComponentBase
{
    [Inject] private ILogger<Company> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    
    [Parameter] public Guid CompanyId { get; set; }
    
    private CompanyViewModel CompanyModel { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsEditMode { get; set; } = false;
    private string ViewUserRole { get; set; } = User;
    
    private EditContext _editContext;
    private ValidationMessageStore _errorMessageStore;
    private List<string> AdditionalInfoRoles { get; set; } = [Admin, CompanyAdmin];
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await TimeZoneHelper.DetectTimeZone();
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;
            
            var result = await CompaniesHttpService.GetCompanyById(CompanyId);
            if (result is not null)
            {
                CompanyModel = result;
                IsLoading = false;
            }
            else
            {
                throw new("company_not_found");
            }

            ViewUserRole = await ((CustomAuthStateProvider)AuthenticationStateProvider).GetUserRole();
            
            IsEditMode = NavigationManager.Uri.EndsWith("/Edit", StringComparison.OrdinalIgnoreCase);
            if (IsEditMode)
            {
                _editContext = new(CompanyModel);
                _errorMessageStore = new(_editContext);
            
                _editContext.OnFieldChanged += (sender, e) =>
                {
                    _errorMessageStore.Clear(e.FieldIdentifier);
                    _editContext.NotifyValidationStateChanged();
                };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            NavigationManager.NavigateTo("/Companies", forceLoad: true);
        }
    }

    private async Task HandleCompanyEdit(EditContext arg)
    {
        try
        {
            _errorMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            
            if (!_editContext.Validate())
                throw new("Input values are invalid");

            var result = await CompaniesHttpService.UpdateCompany(CompanyModel);
            if (result is null)
            {
                _errorMessageStore.Clear();
                _errorMessageStore.Add(new FieldIdentifier(CompanyModel, nameof(CompanyModel.Title)), $"There was error while making a request to a API server");
                
                _editContext.NotifyValidationStateChanged();
                return;
            }
            
            CompanyModel = result;
            StateHasChanged();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            NavigationManager.NavigateTo("/Companies", forceLoad: true);
        }
    }
}