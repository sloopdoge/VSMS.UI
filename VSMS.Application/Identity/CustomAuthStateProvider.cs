using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using VSMS.Domain.Constants;

namespace VSMS.Application.Identity;

/// <summary>
/// Custom authentication state provider for managing user identity using local storage and JWT.
/// Integrates with Blazor's authentication system.
/// </summary>
public class CustomAuthStateProvider(
    ILogger<CustomAuthStateProvider> logger,
    ILocalStorageService localStorage) : AuthenticationStateProvider
{
    /// <summary>
    /// Gets the current authentication state by validating the JWT stored in local storage.
    /// </summary>
    /// <returns>The current <see cref="AuthenticationState"/> based on the stored token.</returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var token = await localStorage.GetItemAsStringAsync(CookieKeys.AuthToken);
            var tokenExpires = await localStorage.GetItemAsync<DateTime>(CookieKeys.AuthTokenExpires);

            if (!string.IsNullOrEmpty(token) && DateTime.UtcNow < tokenExpires)
                identity = new(ParseClaimsFromJwt(token), "jwt");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting authentication state");
        }

        return new(new(identity));
    }

    /// <summary>
    /// Notifies the application that the user has successfully authenticated.
    /// Saves user data and token into local storage and updates the authentication state.
    /// </summary>
    /// <param name="token">JWT token.</param>
    /// <param name="expires">Token expiration date and time (UTC).</param>
    /// <param name="userId">Unique user identifier (GUID).</param>
    /// <param name="userRoleName">User's role name.</param>
    /// <param name="userFullName">User's full name.</param>
    /// <param name="userEmail">User's email address.</param>
    public async Task NotifyUserAuthentication(string token, DateTime expires, Guid userId, string userRoleName, string userFullName, string userEmail)
    {
        try
        {
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            await localStorage.SetItemAsync(CookieKeys.AuthToken, token);
            await localStorage.SetItemAsync(CookieKeys.AuthTokenExpires, expires);
            await localStorage.SetItemAsync(CookieKeys.UserId, userId.ToString());
            await localStorage.SetItemAsync(CookieKeys.UserRoleName, userRoleName);
            await localStorage.SetItemAsync(CookieKeys.UserFullName, userFullName);
            await localStorage.SetItemAsync(CookieKeys.UserEmail, userEmail);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error setting authentication state and user info");
        }
    }

    /// <summary>
    /// Logs the user out by clearing authentication-related data from local storage
    /// and updating the authentication state to unauthenticated.
    /// </summary>
    public async Task NotifyUserLogout()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        await localStorage.RemoveItemsAsync([
            CookieKeys.AuthToken,
            CookieKeys.AuthTokenExpires,
            CookieKeys.UserId,
            CookieKeys.UserRoleName,
            CookieKeys.UserFullName,
            CookieKeys.UserEmail
        ]);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    /// <summary>
    /// Retrieves the role of the currently authenticated user.
    /// </summary>
    /// <returns>User's role name, or an empty string if not authenticated or no role claim is found.</returns>
    public async Task<string> GetUserRole()
    {
        var authState = await GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && string.IsNullOrEmpty(roleClaim.Value))
                return roleClaim.Value;
        }

        return "";
    }

    /// <summary>
    /// Retrieves the ID of the currently authenticated user.
    /// </summary>
    /// <returns>User's GUID identifier, or <see cref="Guid.Empty"/> if not authenticated or claim not found.</returns>
    public async Task<Guid> GetUserId()
    {
        var authState = await GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            var userClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim != null && Guid.TryParse(userClaim.Value, out Guid id))
                return id;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Determines whether the currently authenticated user is in one of the specified roles.
    /// </summary>
    /// <param name="roles">An array of role names to check against.</param>
    /// <returns><c>true</c> if the user is in one of the roles; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsUserInRoles(string[] roles)
    {
        var userRole = await GetUserRole();
        return roles.Contains(userRole);
    }

    /// <summary>
    /// Retrieves the JWT token of the current user from local storage.
    /// </summary>
    /// <returns>The token as a string, or an empty string if not found or an error occurred.</returns>
    public async Task<string> GetToken()
    {
        try
        {
            var token = await localStorage.GetItemAsStringAsync(CookieKeys.AuthToken);
            return string.IsNullOrEmpty(token)
                ? string.Empty
                : token;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return string.Empty;
        }
    }
    
    private static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        var claims = new List<Claim>();
    
        var payload = token.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
            claims.AddRange(keyValuePairs.Select(kvp => 
                new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)));

        return claims;
    }
    
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        
        return Convert.FromBase64String(base64);
    }
}