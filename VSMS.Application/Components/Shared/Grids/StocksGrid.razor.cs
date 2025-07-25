﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Components.Shared.Modals;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
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
    
    private HashSet<StockPerformanceViewModel> Stocks { get; set; } = [];
    private HashSet<StockPerformanceViewModel> SelectedStocks { get; set; } = [];
    private HashSet<StockPerformanceViewModel> FilteredStocks { get; set; } = [];
    private bool IsLoading { get; set; } = true;

    private FilterDefinition<StockPerformanceViewModel> _stocksFilterDefinition;
    private MudDataGrid<StockPerformanceViewModel> _dataGrid;
    private bool _symbolFilterOpened = false;
    private bool _symbolFilterSelectedAll = false;
    
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
            if (!StocksHub.IsConnected)
                await StocksHub.InitializeHub();

            RegisterHubHandlers();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task<GridData<StockPerformanceViewModel>> RefreshGrid(GridState<StockPerformanceViewModel> state)
    {
        IsLoading = true;
        try
        {
            var data = CompanyId != Guid.Empty 
                ? await StocksHttpService.GetStocksPerformanceByCompanyId(CompanyId)
                : await StocksHttpService.GetAllStocksPerformance();
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

    private void RegisterHubHandlers()
    {
        StocksHub.RegisterHandler<List<StockViewModel>>($"OnStocksPriceChanged",
            async (stocks) => _ = OnStocksPriceChanged(stocks));
    }

    private async Task OnStocksPriceChanged(List<StockViewModel> updatedStocks)
    {
        try
        {
            await InvokeAsync(async () =>
            {
                    if (Stocks.Any(s => updatedStocks.Select(us => us.Id).Contains(s.Id)))
                    {
                        await _dataGrid.ReloadServerData();
                        StateHasChanged();
                    }
            });
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private void OpenStockSymbolFilter()
    {
        _symbolFilterOpened = true;
    }

    private void SelectAllFilterStockSymbols(bool value)
    {
        _symbolFilterSelectedAll = true;
        
        if (value)
        {
            SelectedStocks = Stocks.ToHashSet();
        }
        else
        {
            SelectedStocks.Clear();
        }
    }

    private void SelectedFilterStockSymbolChanged(bool value, StockPerformanceViewModel item)
    {
        if (value)
            SelectedStocks.Add(item);
        else
            SelectedStocks.Remove(item);

        _symbolFilterSelectedAll = SelectedStocks.Count == Stocks.Count;
    }

    private async Task ClearStockSymbolFilter(FilterContext<StockPerformanceViewModel> context)
    {
        SelectedStocks = Stocks.ToHashSet();
        FilteredStocks = Stocks.ToHashSet();
        await context.Actions.ClearFilterAsync(_stocksFilterDefinition);
        _symbolFilterOpened = false;
    }

    private async Task ApplyStockSymbolFilter(FilterContext<StockPerformanceViewModel> context)
    {
        FilteredStocks = SelectedStocks;
        await context.Actions.ApplyFilterAsync(_stocksFilterDefinition);
        _symbolFilterOpened = false;
    }

    private async Task OnCreateStockClick(MouseEventArgs arg)
    {
        try
        {
            var parameters = new DialogParameters<StockCreateModal>
            {
                { x => x.CompanyId, CompanyId },
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
            
            var dialogReference = await DialogService.ShowAsync<StockCreateModal>(@Localizer["company_stocks_title"], parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult is { Canceled: false })
                await _dataGrid.ReloadServerData();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}