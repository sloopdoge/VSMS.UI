using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Components.Shared.Modals;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Domain.Models.ViewModels.Shared.Filters;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Pages.Companies;

public partial class Companies : ComponentBase
{
    [Inject] private ILogger<Companies> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    
    private MudDataGrid<CompanyViewModel> DataGrid { get; set; } = new();
    private List<CompanyViewModel> Items { get; set; } = [];
    private string SearchString { get; set; } = "";
    private bool IsLoading { get; set; } = false;
    private CompaniesFilterViewModel Filter { get; set; } = new(){ SortBy = nameof(CompanyViewModel.CreatedAt)};

    private async Task<GridData<CompanyViewModel>> LoadServerData(GridState<CompanyViewModel> state)
    {
        IsLoading = true;

        try
        {
            Filter.Page = state.Page + 1;
            Filter.PageSize = state.PageSize;

            var sort = state.SortDefinitions.FirstOrDefault();
            if (sort is not null)
            {
                Filter.SortBy = sort.SortBy;
                Filter.SortAscending = !sort.Descending;
            }

            var result = await CompaniesHttpService.GetCompaniesByFilter(Filter);

            if (result is null)
                return new GridData<CompanyViewModel> { Items = [], TotalItems = 0 };

            return new GridData<CompanyViewModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount
            };
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading companies with filter");
            return new GridData<CompanyViewModel> { Items = [], TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task OnSearch(string text)
    {
        SearchString = text;
        return DataGrid.ReloadServerData();
    }

    private void OnEditClick(Guid companyId)
    {
        NavigationManager.NavigateTo($"/Company/{companyId}/Edit");
    }

    private async Task OnDeleteClick(Guid companyId)
    {
        try
        {
            var isConfirmed = await ShowConfirmModal();
            if (!isConfirmed) return;
            
            var res = await CompaniesHttpService.DeleteCompanyById(companyId);
            if (res)
                await DataGrid.ReloadServerData();
            else
                Logger.LogError($"Failed to delete company {companyId}");
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task OnCreateCompanyClick()
    {
        try
        {
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
            
            var dialogReference = await DialogService.ShowAsync<CompanyViewModal>(@Localizer["companies_grid_modal_text"], options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await DataGrid.ReloadServerData();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private void OnRowClick(Guid itemId)
    {
        NavigationManager.NavigateTo($"/Company/{itemId}");
    }
    
    private async Task<bool> ShowConfirmModal()
    {
        try
        {
            var parameters = new DialogParameters<ConfirmationModal>
            {
                { x => x.Message, $"company_delete_confirmation_message" },
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