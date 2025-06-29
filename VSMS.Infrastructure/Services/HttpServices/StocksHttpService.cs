using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Services.HttpServices;

public class StocksHttpService(
    ILogger<StocksHttpService> logger,
    ILocalStorageService localStorage,
    IApplicationSettings applicationSettings) : BaseHttpService(localStorage, applicationSettings, "Stocks")
{
    public async Task<StockViewModel?> GetStockById(Guid stockId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"{stockId}"));
            var content = await result.Content.ReadFromJsonAsync<StockViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<List<StockViewModel>?> GetAllStocks()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url());
            if (!result.IsSuccessStatusCode)
                return null;
            
            var content = await result.Content.ReadFromJsonAsync<List<StockViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<StockViewModel?> CreateStock(StockViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.PostAsJsonAsync(Url(), model);
            var content = await result.Content.ReadFromJsonAsync<StockViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<StockViewModel?> UpdateStock(StockViewModel model)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.PutAsJsonAsync(Url(), model);
            var content = await result.Content.ReadFromJsonAsync<StockViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
    
    public async Task<bool> DeleteStock(Guid stockId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.DeleteAsync(Url($"{stockId}"));
            var content = result.IsSuccessStatusCode && await result.Content.ReadFromJsonAsync<bool>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
    
    public async Task<StockPerformanceViewModel?> GetStocksPerformanceById(Guid stockId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"StocksPerformance/{stockId}"));
            var content = await result.Content.ReadFromJsonAsync<StockPerformanceViewModel>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<List<StockPerformanceViewModel>?> GetStocksPerformanceByCompanyId(Guid companyId)
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"StocksPerformance/Company/{companyId}"));
            var content = await result.Content.ReadFromJsonAsync<List<StockPerformanceViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task<List<StockPerformanceViewModel>?> GetAllStocksPerformance()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"StocksPerformance"));
            var content = await result.Content.ReadFromJsonAsync<List<StockPerformanceViewModel>>();
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
}