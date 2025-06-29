using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Hubs;

public class StocksHub(
    IApplicationSettings settings, 
    ILogger<StocksHub> logger,
    ILocalStorageService localStorage) 
    : BaseHub(settings, logger, localStorage, "StocksHub")
{
    public async Task<List<StockViewModel>?> GetStockHistoryById(Guid stockId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (HubConnection is null)
                throw new InvalidOperationException($"{nameof(StocksHub)} connection is not initialized.");

            return await GetValue(await HubConnection.InvokeAsync<ResponseModel<List<StockViewModel>>>(
                "GetStockHistoryById", stockId, startDate, endDate));
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
}