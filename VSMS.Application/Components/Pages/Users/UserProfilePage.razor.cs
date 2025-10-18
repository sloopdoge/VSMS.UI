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

namespace VSMS.Application.Components.Pages.Users;

public partial class UserProfilePage : ComponentBase
{
    [Inject] private ILogger<UserProfilePage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] private UsersHttpService UsersHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    [Parameter] public Guid UserId { get; set; }
    
    private bool IsLoading { get; set; } = true;
    private UserProfileViewModel UserProfile { get; set; } = new();
    private bool IsEditMode { get; set; } = false;
    private string ViewUserRole { get; set; } = User;
    
    private EditContext _editContext;
    private ValidationMessageStore _errorMessageStore;
    private List<string> AdditionalInfoRoles = [Admin, CompanyAdmin];
    private List<string> AvailableRoles = [CompanyAdmin, CompanyManager];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (UserId == Guid.Empty)
            {
                Logger.LogWarning("Invalid UserId in route");
                NavigationManager.NavigateTo("/Users");
                return;
            }
            
            ViewUserRole = await ((CustomAuthStateProvider)AuthenticationStateProvider).GetUserRole();

            var userProfile = await UsersHttpService.GetUserProfileById(UserId);
            if (userProfile is null)
            {
                Logger.LogWarning("UserProfile not found");
                NavigationManager.NavigateTo("/Users/");
                return;
            }

            UserProfile = userProfile;

            IsEditMode = NavigationManager.Uri.EndsWith("/Edit", StringComparison.OrdinalIgnoreCase);
            if (IsEditMode)
            {
                _editContext = new(UserProfile);
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
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleUserEdit()
    {
        try
        {
            if (!_editContext.IsModified())
                return;
            
            var editRes = await UsersHttpService.UpdateUser(UserProfile);
            if (editRes is null)
                throw new Exception($"User {UserProfile.Email} was not updated");
            
            UserProfile = editRes;
            StateHasChanged();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private void OnCancelClick()
    {
        NavigationManager.NavigateTo($"/User/{UserId}");
    }
}