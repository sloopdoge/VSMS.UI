using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Components.Shared.Modals;
using VSMS.Domain;
using VSMS.Domain.Enums;
using VSMS.Domain.Models.ViewModels;
using VSMS.Domain.Models.ViewModels.Shared.Filters;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Shared.Grids;

public partial class StocksGrid : ComponentBase
{
    [Inject] private ILogger<StocksGrid> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private StocksHub StocksHub { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    
    [Parameter] public Guid CompanyId { get; set; } = Guid.Empty;
    [Parameter] public bool DirectToStockPage { get; set; } = false;
    [Parameter] public bool ShowCreateStockButton { get; set; } = false;
    
    private MudDataGrid<StockViewModel> DataGrid { get; set; } = new();
    private StocksFilterViewModel Filter { get; set; } = new() { SortBy = nameof(StockViewModel.CreatedAt) };
    private bool IsLoading { get; set; } = true;
    
    private async Task<GridData<StockViewModel>> LoadServerData(GridState<StockViewModel> state)
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

            if (CompanyId != Guid.Empty)
                Filter.CompanyIds = [CompanyId];

            var result = await StocksHttpService.GetStocksByFilter(Filter);

            if (result is null)
                return new GridData<StockViewModel> { Items = [], TotalItems = 0 };

            return new GridData<StockViewModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount
            };
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading stocks with filter");
            return new GridData<StockViewModel> { Items = [], TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnSearch(string search)
    {
        Filter.Search = search;
        await DataGrid.ReloadServerData();
    }
    
    private async Task OpenCreateStockModal()
    {
        try
        {
            var parameters = new DialogParameters<StockViewModal>
            {
                { x => x.CompanyId, CompanyId },
                { x => x.ModalMode, ModalModeEnum.Create }
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
            
            var dialogReference = await DialogService.ShowAsync<StockViewModal>(@Localizer["company_stocks_title"], parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await DataGrid.ReloadServerData();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
    
    private async Task OnEditClick(Guid stockId)
    {
        try
        {
            var parameters = new DialogParameters<StockViewModal>
            {
                { x => x.CompanyId, CompanyId },
                { x => x.StockId, stockId},
                { x => x.ModalMode, ModalModeEnum.Edit}
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
            
            var dialogReference = await DialogService.ShowAsync<StockViewModal>(@Localizer["company_stocks_title"], parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await DataGrid.ReloadServerData();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task OnDeleteClick(Guid stockId)
    {
        try
        {
            var isConfirmed = await ShowConfirmModal();
            if (!isConfirmed) return;
            
            var res = await StocksHttpService.DeleteStock(stockId);
            if (res)
                await DataGrid.ReloadServerData();
            else
                Logger.LogError($"Failed to delete stock {stockId}");
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
                { x => x.Message, $"stock_delete_confirmation_message" },
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