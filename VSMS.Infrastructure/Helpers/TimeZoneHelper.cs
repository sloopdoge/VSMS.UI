using Microsoft.JSInterop;

namespace VSMS.Infrastructure.Helpers;

/// <summary>
/// Helper class for detecting the user's time zone using JavaScript interop in Blazor.
/// </summary>
public class TimeZoneHelper(IJSRuntime jsRuntime)
{
    /// <summary>
    /// Gets the time zone identifier of the user (e.g., "Europe/Kyiv").
    /// </summary>
    public string UserTimeZone { get; private set; }

    /// <summary>
    /// Detects the user's current time zone using JavaScript and stores the result.
    /// </summary>
    /// <returns>The detected time zone identifier as a string.</returns>
    public async Task<string> DetectTimeZone()
    {
        UserTimeZone = await jsRuntime.InvokeAsync<string>("timezoneInterop.getTimeZone");
        return UserTimeZone;
    }
}
