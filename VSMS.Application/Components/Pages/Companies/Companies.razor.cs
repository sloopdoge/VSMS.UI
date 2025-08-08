using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Components.Shared.Modals;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
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
    
    private MudDataGrid<CompanyViewModel> _dataGrid;
    private string SearchString { get; set; } = "";
    private bool IsLoading { get; set; } = false;

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

    private async Task<GridData<CompanyViewModel>> RefreshGrid(GridState<CompanyViewModel> state)
    {
        IsLoading = true;
        try
        {
            var data = await CompaniesHttpService.GetAllCompanies();
            if (data is null)
            {
                return new()
                {
                    TotalItems = 0,
                    Items = []
                };
            }

            data = data.Where(company =>
            {
                if (string.IsNullOrWhiteSpace(SearchString))
                    return true;
                if (company.Title.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }).ToList();
            var totalItems = data.Count();

            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition is not null)
            {
                data = sortDefinition.SortBy switch
                {
                    nameof(CompanyViewModel.Title) => data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending, 
                            c => c.Title)
                        .ToList(),
                    nameof(CompanyViewModel.UpdatedAt) => data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            c => c.UpdatedAt)
                        .ToList(),
                    nameof(CompanyViewModel.CreatedAt) => data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            c => c.CreatedAt)
                        .ToList()
                };
            }

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

    private Task OnSearch(string text)
    {
        SearchString = text;
        return _dataGrid.ReloadServerData();
    }

    private void OnEditClick(Guid companyId)
    {
        NavigationManager.NavigateTo($"/Company/{companyId}/Edit");
    }

    private async Task OnDeleteClick(Guid companyId)
    {
        try
        {
            var res = await CompaniesHttpService.DeleteCompanyById(companyId);
            if (res)
                await _dataGrid.ReloadServerData();
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
                await _dataGrid.ReloadServerData();
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
}