﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Extensions;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Pages.Stocks;

public partial class Stock : ComponentBase
{
    [Inject] private ILogger<Stock> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }
    [Inject] private StocksHub StocksHub { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    
    [Parameter] public Guid StockId { get; set; }

    private StockViewModel StockModel { get; set; } = new();
    private List<StockViewModel> StockHistory { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-1);
    private DateTime EndDate { get; set; } = DateTime.UtcNow;
    private TimeSpan SeriesSpacing { get; set; }
    
    private List<TimeSeriesChartSeries> _series = new();
    private readonly ChartOptions _options = new()
    {
        YAxisLines = false,
        YAxisTicks = 5,
        MaxNumYAxisTicks = 10,
        YAxisRequireZeroPoint = false,
        XAxisLines = false,
        LineStrokeWidth = 3,
    };

    private readonly AxisChartOptions _axisChartOptions = new()
    {
        MatchBoundsToSize = true,
        StackedBarWidthRatio = 1,
    };


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
        IsLoading = true;
        try
        {
            if (!StocksHub.IsConnected)
                await StocksHub.InitializeHub();
            
            var stockRes = await StocksHttpService.GetStockById(StockId);
            if (stockRes is not null)
            {
                StockModel = stockRes;

                var stockHistoryRes = await StocksHub.GetStockHistoryById(StockId, StartDate, EndDate);
                if (stockHistoryRes is not null)
                {
                    StockHistory = stockHistoryRes;
                    
                    _series =
                    [
                        new()
                        {
                            Index = 0,
                            Name = Localizer["stock_details_timeserieschart_series_name"],
                            Data = StockHistory.Select(x =>
                                new TimeSeriesChartSeries.TimeValue(
                                    x.UpdatedAt.ConvertUtcToLocal(TimeZoneHelper.UserTimeZone), 
                                    (double)x.Price)).ToList(),
                            IsVisible = true,
                            LineDisplayType = LineDisplayType.Line,
                            DataMarkerTooltipTitleFormat = "{{X_VALUE}}",
                            DataMarkerTooltipSubtitleFormat = "{{Y_VALUE}}"
                        }
                    ];

                    SetChartSeriesParameters();
                    
                    StateHasChanged();
                }
                else
                    Logger.LogWarning("Stock history not found");
            }
            else
                Logger.LogWarning("Stock not found");
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

    private void SetChartSeriesParameters()
    {
        try
        {
            var minDate = StockHistory.Min(x => x.UpdatedAt);
            var maxDate = StockHistory.Max(x => x.UpdatedAt);
            var minPrice = StockHistory.Min(x => x.Price);
            var maxPrice = StockHistory.Max(x => x.Price);
            
            SeriesSpacing = TimeSpan.FromMilliseconds((maxDate - minDate).Add(TimeSpan.FromMinutes(30)).TotalMilliseconds);

            var yTicks = (int)(maxPrice - minPrice) / 5;
            
            _options.YAxisTicks = yTicks <= 0 
                ? 1 
                : yTicks;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task RefreshStockHistory()
    {
        
    }
}