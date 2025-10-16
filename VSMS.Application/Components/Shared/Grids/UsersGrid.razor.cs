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
    private bool IsLoading { get; set; } = true;
    

    private FilterDefinition<UserProfileViewModel> _usersFilterDefinition;
    private MudDataGrid<UserProfileViewModel> _dataGrid;
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

    private async Task<GridData<UserProfileViewModel>> RefreshGrid(GridState<UserProfileViewModel> state)
    {
        IsLoading = true;
        try
        {
            var data = CompanyId == Guid.Empty
                ? await UsersHttpService.GetAllUserProfiles()
                : await CompaniesHttpService.GetAllUsersInCompany(CompanyId);
            if (data is null)
            {
                return new()
                {
                    TotalItems = 0,
                    Items = []
                };
            }
            
            var totalItems = data.Count();

            var pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToList();
            return new()
            {
                TotalItems = totalItems,
                Items = pagedData,
            };
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return new();
        }
        finally
        {
            IsLoading = false;
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
            var parameters = new DialogParameters<UserViewModal>()
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
            
            var dialogReference = await DialogService.ShowAsync<UserViewModal>(@Localizer["users_title"], parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await _dataGrid.ReloadServerData();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private void OnEditClick(Guid userId)
    {
        NavigationManager.NavigateTo($"/User/{userId}/Edit");
    }

    private async Task OnDeleteClick(Guid userId)
    {
        try
        {
            var isConfirmed = await ShowConfirmModal();
            if (!isConfirmed) return;
            
            var res = await UsersHttpService.DeleteUser(userId);
            if (res)
                await _dataGrid.ReloadServerData();
            else
                Logger.LogError($"Failed to delete user {userId}");
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
    
    private async Task<bool> ShowConfirmModal()
    {
        try
        {
            var parameters = new DialogParameters<ConfirmationModal>
            {
                { x => x.Message, $"user_delete_confirmation_message" },
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
            
            var dialogReference = await DialogService.ShowAsync<ConfirmationModal>("", parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: true })
                return false;
            
            return dialogResult?.Data is bool;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }
}