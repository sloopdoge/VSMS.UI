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
    /// <summary>
    /// Retrieves a specific stock by its identifier from the API.
    /// </summary>
    /// <param name="stockId">The unique identifier of the stock.</param>
    /// <returns>The stock view model if found; otherwise, <c>null</c>.</returns>
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
    
    /// <summary>
    /// Retrieves the collection of all stocks.
    /// </summary>
    /// <returns>A list of stocks or <c>null</c> on failure.</returns>
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
    
    /// <summary>
    /// Creates a new stock entry via the API.
    /// </summary>
    /// <param name="model">The stock details.</param>
    /// <returns>The created stock model or <c>null</c> on failure.</returns>
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
    
    /// <summary>
    /// Updates an existing stock with new information.
    /// </summary>
    /// <param name="model">The updated stock information.</param>
    /// <returns>The updated stock or <c>null</c> on failure.</returns>
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
    
    /// <summary>
    /// Deletes the specified stock from the system.
    /// </summary>
    /// <param name="stockId">The identifier of the stock to delete.</param>
    /// <returns><c>true</c> if deletion succeeds; otherwise, <c>false</c>.</returns>
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
    
    /// <summary>
    /// Retrieves performance information for a specific stock.
    /// </summary>
    /// <param name="stockId">The identifier of the stock.</param>
    /// <returns>The stock performance view model or <c>null</c> on failure.</returns>
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

    /// <summary>
    /// Retrieves performance statistics for all stocks of a particular company.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <returns>List of stock performance models or <c>null</c> on failure.</returns>
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

    /// <summary>
    /// Retrieves performance information for all stocks.
    /// </summary>
    /// <returns>List of all stock performance models or <c>null</c> on failure.</returns>
    public async Task<List<StockPerformanceViewModel>?> GetAllStocksPerformance()
    {
        try
        {
            await AddAuthorizationAsync();
            var result = await Client.GetAsync(Url($"StocksPerformance"));
            var content = await result.Content.ReadFromJsonAsync<List<StockPerformanceViewModel>>();
            
            content?.ForEach(stock => stock.SetAdditionalInfo());
            
            return content;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return null;
        }
    }
}