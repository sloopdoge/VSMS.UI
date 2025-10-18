using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using VSMS.Application.Identity;
using VSMS.Domain;
using VSMS.Domain.Constants;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private ILogger<MainLayout> Logger { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    [Inject] private AuthHttpService AuthHttpService { get; set; }
    [Inject] private WebPageHelper WebPageHelper { get; set; }
    [Inject] private ApplicationHub ApplicationHub { get; set; }
    [Inject] private CookieHelper CookieHelper { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }

    private AuthenticationState AuthenticationState { get; set; }
    private bool DarkThemeState { get; set; } = true;
    private string CurrentCulture { get; set; } = "en-us";
    
    private string[] _supportedCultures = ["en-us", "uk-ua"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                if (!await CheckUserCulture())
                    return;

                if(!await CheckUserThemeState())
                    return;

                if (!await CheckAuthState())
                {
#if DEBUG
                    Logger.LogWarning($"User is not authenticated");
#endif
                    return;
                }

                if (AuthenticationState.User.Identity?.IsAuthenticated ?? false)
                    if (!await CheckHubConnections())
                    {
#if DEBUG
                        Logger.LogError($"There was error checking for hub connections");
#endif
                        return;
                    }

                await TimeZoneHelper.DetectTimeZone();
                
                StateHasChanged();
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
        }
    }

    #region Checkers

    private async Task<bool> CheckAuthState()
    {
        try
        {
            AuthenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (AuthenticationState?.User?.Identity?.IsAuthenticated ?? false)
            {
                var token = await ((CustomAuthStateProvider)AuthenticationStateProvider).GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenValidationResult = await AuthHttpService.ValidateToken();
                    if (tokenValidationResult is { IsValid: true })
                        return true;
                }
            }
#if DEBUG
            Logger.LogWarning($"{AuthenticationState?.User?.Identity?.Name ?? "Anonymous user"} is not authenticated");
#endif
            return !await HandleLogout();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            NavigationManager.NavigateTo("/", forceLoad: true);
            return false;
        }
    }

    private async Task<bool> CheckUserCulture()
    {
        try
        {
            var userCulture = await LocalStorage.GetItemAsync<string>(CookieKeys.Culture);
            if (string.Equals(CurrentCulture, userCulture) && !string.IsNullOrEmpty(userCulture)) 
                return true;
            
            if (!_supportedCultures.Any(c => string.Equals(c, userCulture)))
                userCulture = null;
            
            return userCulture is not null 
                ? await HandleUserCultureChange(userCulture)
                : await HandleUserCultureChange();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }

    private async Task<bool> CheckUserThemeState()
    {
        try
        {
            var darkThemeState = await LocalStorage.GetItemAsync<bool?>(CookieKeys.DarkThemeState) ?? true;
            if (DarkThemeState == darkThemeState) return true;
            
            return await HandleThemeStateChange(darkThemeState);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }
    
    
    private async Task<bool> CheckHubConnections()
    {
        try
        {
            if (!ApplicationHub.IsConnected)
                await ApplicationHub.InitializeHub();
            
            return ApplicationHub.IsConnected;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }

    #endregion

    #region Handlers
    
    private async Task<bool> HandleLogout()
    {
        try
        {
            await ((CustomAuthStateProvider)AuthenticationStateProvider).NotifyUserLogout();
            NavigationManager.NavigateTo("/Login");
            StateHasChanged();
            
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }
    
    private async Task<bool> HandleUserCultureChange(string culture = "en-us", bool reload = false)
    {
        try
        {
            CurrentCulture = culture;

            var cultureInfo = new CultureInfo(CurrentCulture);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            await LocalStorage.SetItemAsync(CookieKeys.Culture, CurrentCulture);
            var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            await CookieHelper.SetCookie(CookieRequestCultureProvider.DefaultCookieName, cultureCookieValue, TimeSpan.FromDays(365).Seconds);
            
            if (reload)
                await WebPageHelper.ReloadPage();

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }


    private async Task<bool> HandleThemeStateChange(bool themeState, bool reload = false)
    {
        try
        {
            if (DarkThemeState == themeState) return true;

            DarkThemeState = themeState;
            await LocalStorage.SetItemAsync(CookieKeys.DarkThemeState, themeState);
            if (reload)
                await InvokeAsync(StateHasChanged);

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            return false;
        }
    }

    #endregion
}