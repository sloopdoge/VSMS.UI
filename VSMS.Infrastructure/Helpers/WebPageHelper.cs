using Microsoft.JSInterop;

namespace VSMS.Infrastructure.Helpers;

/// <summary>
/// Helper class for performing common browser page actions using JavaScript interop in Blazor.
/// </summary>
public class WebPageHelper(IJSRuntime jsRuntime)
{
    /// <summary>
    /// Navigates to the previous page in the browser history using JavaScript.
    /// </summary>
    public async Task GoBack()
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        await jsRuntime.InvokeVoidAsync("webHelper.goBack");
    }

    /// <summary>
    /// Reloads the current browser page using JavaScript.
    /// </summary>
    public async Task ReloadPage()
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        await jsRuntime.InvokeVoidAsync("webHelper.reloadPage");
    }

    /// <summary>
    /// Reloads the current page after a reconnection event using JavaScript.
    /// Useful for restoring application state after a lost connection.
    /// </summary>
    public async Task ReloadAfterReconnect()
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        await jsRuntime.InvokeVoidAsync("webHelper.reloadAfterReconnect");
    }
}
