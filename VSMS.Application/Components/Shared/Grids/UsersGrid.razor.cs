using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Components.Shared.Modals;
using VSMS.Domain;
using VSMS.Domain.Constants;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Shared.Grids;

public partial class UsersGrid : ComponentBase
{
    [Inject] private ILogger<StocksGrid> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    [Inject] private UsersHttpService UsersHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    
    [Parameter] public Guid CompanyId { get; set; } = Guid.Empty;
    [Parameter] public bool DirectToUserPage { get; set; } = false;
    [Parameter] public bool ShowCreateUserButton { get; set; } = false;
    
    private HashSet<UserProfileViewModel> Users { get; set; } = [];
    private HashSet<UserProfileViewModel> SelectedUsers { get; set; } = [];
    private HashSet<UserProfileViewModel> FilteredUsers { get; set; } = [];
    private List<string> SelectedFilterUserRoles { get; set; } = [];
    private bool UsersLoading { get; set; } = true;

    private FilterDefinition<UserProfileViewModel> _usersFilterDefinition;
    private bool _symbolFilterOpened = false;
    private bool _symbolFilterSelectedAll = false;
    private List<string> _availableUserRolesForFilter = [RoleNames.User, RoleNames.CompanyManager, RoleNames.CompanyAdmin];
    
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
            UsersLoading = true;
            await RefreshUsers();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task RefreshUsers()
    {
        try
        {
            UsersLoading = true;

            var users = CompanyId != Guid.Empty
                ? await CompaniesHttpService.GetAllUsersInCompany(CompanyId)
                : await UsersHttpService.GetAllUserProfiles();
            if (users is not null)
            {
                Users = users.ToHashSet();

                SelectedUsers = Users.ToHashSet();
                FilteredUsers = Users.ToHashSet();
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
        finally
        {
            UsersLoading = false;
        }
    }
    
    private void OpenUsersRolesFilter()
    {
        _symbolFilterOpened = true;
    }

    private void SelectAllFilterUsersRoles(bool value)
    {
        _symbolFilterSelectedAll = true;
        
        if (value)
        {
            SelectedUsers = Users.ToHashSet();
        }
        else
        {
            SelectedUsers.Clear();
        }
    }

    private void SelectedFilterUsersRoleChanged(bool value, string item)
    {
        
        if (value)
        {
            SelectedFilterUserRoles.Add(item);
            
            var rangeToAdd = Users
                .Where(u => string.Equals(u.Role, item, StringComparison.OrdinalIgnoreCase));
            
            rangeToAdd.ToList().ForEach(u => SelectedUsers.Add(u));
        }
        else
        {
            SelectedFilterUserRoles.Remove(item);
            
            var rangeToRemove = Users
                .Where(u => string.Equals(u.Role, item, StringComparison.OrdinalIgnoreCase));
            
            rangeToRemove.ToList().ForEach(u => SelectedUsers.Remove(u));
        }

        _symbolFilterSelectedAll = SelectedUsers.Count == Users.Count;
    }

    private async Task ClearStockSymbolFilter(FilterContext<UserProfileViewModel> context)
    {
        SelectedUsers = Users.ToHashSet();
        FilteredUsers = Users.ToHashSet();
        await context.Actions.ClearFilterAsync(_usersFilterDefinition);
        _symbolFilterOpened = false;
    }

    private async Task ApplyStockSymbolFilter(FilterContext<UserProfileViewModel> context)
    {
        FilteredUsers = SelectedUsers;
        await context.Actions.ApplyFilterAsync(_usersFilterDefinition);
        _symbolFilterOpened = false;
    }

    private async Task OnCreateUserClick(MouseEventArgs arg)
    {
        try
        {
            var parameters = new DialogParameters<UserCreateModal>()
            {
                { x => x.CompanyId, CompanyId }
            };
            
            
            var options = new DialogOptions
            {
                Position = DialogPosition.Center,
                CloseOnEscapeKey = true,
                NoHeader = false,
                CloseButton = true,
                FullScreen = false,
                FullWidth = false,
                MaxWidth = MaxWidth.Large,
            };
            
            var dialogReference = await DialogService.ShowAsync<UserCreateModal>(@Localizer["users_title"], parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await RefreshUsers();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}