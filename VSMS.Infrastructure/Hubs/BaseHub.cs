using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSMS.Domain;
using VSMS.Domain.Constants;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Hubs;

public class BaseHub(
    IApplicationSettings settings, 
    ILogger<BaseHub> logger,
    ILocalStorageService localStorage, string hubUrl)
{
    protected HubConnection? HubConnection;
    private string _hubUrl = $"{settings.ApiUrl}/{hubUrl}";
    
    public bool IsConnected => HubConnection?.State == HubConnectionState.Connected;
    
    public async Task<bool> InitializeHub()
    {
        try
        {
            var authToken = await localStorage.GetItemAsync<string>(CookieKeys.AuthToken);
            
            HubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(authToken);
                })
                .AddJsonProtocol()
                .WithAutomaticReconnect()
                .Build();

            HubConnection.Closed += async (error) =>
            {
                logger.LogWarning($"Hub connection id: {HubConnection.ConnectionId} closed: {error?.Message}");
                await ReconnectAsync();
            };

            HubConnection.HandshakeTimeout = TimeSpan.FromSeconds(30);
            
            await HubConnection.StartAsync();
            logger.LogDebug($"Connection to {_hubUrl} with id: {HubConnection.ConnectionId} started successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to initialize Hub connection to {_hubUrl}.");
            return false;
        }
    }
    
    public async Task StopHubAsync()
    {
        if (HubConnection is { State: HubConnectionState.Connected })
        {
            await HubConnection.StopAsync();
            logger.LogDebug($"Connection to {_hubUrl} with id: {HubConnection.ConnectionId} stopped.");
        }
    }

    private async Task ReconnectAsync()
    {
        while (HubConnection?.State != HubConnectionState.Connected)
        {
            try
            {
                await Task.Delay(5000);
                await HubConnection?.StartAsync();
                logger.LogDebug($"Reconnected to {_hubUrl} successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reconnection attempt failed.");
            }
        }
    }

    public void RegisterHandler<T>(string methodName, Action<T> handler)
    {
        HubConnection?.On(methodName, handler);
    }

    public Task<TResult?> GetValue<TResult>(ResponseModel<TResult>? response)
    {
        if (response is null) return Task.FromResult<TResult?>(default);
        
        if (response.Succeeded) return Task.FromResult(response.Value);

        if (response.Error is not null)
            logger.LogError(response.Error.Property, string.Join(". ", response.Error.Description));
        else
            logger.LogError($"There was error withing processing response in Hub");
        
        return Task.FromResult<TResult?>(default);
    }
}