using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using VSMS.Domain;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Pages.Stocks;

public partial class StocksPage : ComponentBase
{
    [Inject] private ILogger<StocksPage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private StocksHttpService StocksHttpService { get; set; }
    [Inject] private StocksHub StocksHub { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    
    private bool IsLoading { get; set; } = true;
}