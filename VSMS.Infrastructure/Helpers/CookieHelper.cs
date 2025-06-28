using Microsoft.JSInterop;

namespace VSMS.Infrastructure.Helpers;

/// <summary>
/// Helper class for managing browser cookies using JavaScript interop in Blazor.
/// </summary>
public class CookieHelper(IJSRuntime jsRuntime)
{
    /// <summary>
    /// Sets a browser cookie using JavaScript.
    /// </summary>
    /// <param name="name">The name of the cookie.</param>
    /// <param name="value">The value to store in the cookie.</param>
    /// <param name="seconds">The lifetime of the cookie in seconds.</param>
    public async Task SetCookie(string name, string value, int seconds)
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        await jsRuntime.InvokeVoidAsync("cookieHelper.setCookie", name, value, seconds);
    }

    /// <summary>
    /// Retrieves the value of a browser cookie using JavaScript.
    /// </summary>
    /// <param name="name">The name of the cookie to retrieve.</param>
    /// <returns>The cookie value as a string.</returns>
    public async Task<string> GetCookie(string name)
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        return await jsRuntime.InvokeAsync<string>("cookieHelper.getCookie", name);
    }

    /// <summary>
    /// Deletes a browser cookie using JavaScript.
    /// </summary>
    /// <param name="name">The name of the cookie to delete.</param>
    public async Task DeleteCookie(string name)
    {
        if (jsRuntime == null) throw new("JSRuntime not initialized");
        await jsRuntime.InvokeVoidAsync("cookieHelper.deleteCookie", name);
    }
}
