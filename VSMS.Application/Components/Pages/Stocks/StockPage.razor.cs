using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Application.Identity;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Extensions;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;
using static VSMS.Domain.Constants.RoleNames;

namespace VSMS.Application.Components.Pages.Stocks;

public partial class StockPage : ComponentBase
{
    [Inject] private ILogger<StockPage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }
    [Inject] private StocksHub StocksHub { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    
    [Parameter] public Guid StockId { get; set; }

    private StockViewModel StockModel { get; set; } = new();
    private List<StockViewModel> StockHistory { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-1);
    private DateTime EndDate { get; set; } = DateTime.UtcNow;
    private TimeSpan SeriesSpacing { get; set; }
    private string ViewUserRole { get; set; } = User;
    
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
        IsLoading = true;
        try
        {
            if (!StocksHub.IsConnected)
            {
                var hubInitialized = await StocksHub.InitializeHub();
                if (!hubInitialized)
                    throw new Exception($"{nameof(StocksHub)} not initialized");

                RegisterHubHandlers();
            }
            
            ViewUserRole = await ((CustomAuthStateProvider)AuthenticationStateProvider).GetUserRole();
            
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
                        new TimeSeriesChartSeries
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

    private async Task OnStocksPriceChanged(List<StockViewModel> updatedStocks)
    {
        try
        {
            await InvokeAsync(async () =>
            {
                if (StockHistory.Count == 0)
                    return;

                var targetId = StockHistory.First().Id;

                foreach (var updated in updatedStocks
                             .Where(updated => updated.Id == targetId && StockHistory
                                 .All(h => h.UpdatedAt != updated.UpdatedAt)))
                {
                    StockHistory.Add(updated);
                }
                
                _series.FirstOrDefault().Data = StockHistory.Select(x =>
                    new TimeSeriesChartSeries.TimeValue(
                        x.UpdatedAt.ConvertUtcToLocal(TimeZoneHelper.UserTimeZone),
                        (double)x.Price)).ToList();
                
                StateHasChanged();
            });
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
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

    private void RegisterHubHandlers()
    {
        StocksHub.RegisterHandler<List<StockViewModel>>($"OnStocksPriceChanged",
            async (stocks) => _ = OnStocksPriceChanged(stocks));
    }
}