using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using VSMS.Domain;
using VSMS.Domain.Enums;
using VSMS.Domain.Resources.Icons;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Settings;

namespace VSMS.Application.Components.Shared;

public partial class Header : ComponentBase
{
    [Inject] private ILogger<Header> Logger { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private WebPageHelper WebPageHelper { get; set; }
    [Inject] private IApplicationSettings ApplicationSettings { get; set; }

    [Parameter] 
    public bool DarkThemeState 
    { 
        get => _darkThemeState;
        set
        {
            if (_darkThemeState != value)
            {
                _darkThemeState = value;
                OnThemeStateChange.InvokeAsync(value);
            }
        } 
    }
    [Parameter] public string UserCulture { get; set; } = "en-us";
    [Parameter] public EventCallback<bool> OnThemeStateChange { get; set; }
    [Parameter] public EventCallback<string> OnUserCultureChange { get; set; }
    
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }
    
    private bool IsAuthenticated { get; set; }
    private bool _darkThemeState = false;
    private Dictionary<string, string> _availableCultures = new()
    {
        {
            "en-us", CountryFlagsIcons.en_us
        },
        {
            "uk-ua", CountryFlagsIcons.uk_ua
        }
    };
    private string ApplicationTitle { get; set; } = "VSMS";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ApplicationTitle = ApplicationSettings.ApplicationTitle.ToUpperInvariant();
            
            var authState = await AuthenticationStateTask;
            IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
        }
    }

    private void OnLogOut()
    {
        NavigationManager.NavigateTo("/Logout");
    }

    private Task ChangeUserCulture(string culture)
    {
        return OnUserCultureChange.InvokeAsync(culture);
    }
    
    private Task ChangeThemeState(bool state)
    {
        return OnThemeStateChange.InvokeAsync(state);
    }

    private void OnNavigationClick(NavMenuItemsEnum item)
    {
        switch (item)
        {
            case NavMenuItemsEnum.Home:
            default:
                NavigationManager.NavigateTo("/");
                break;
            case NavMenuItemsEnum.Companies:
                NavigationManager.NavigateTo("/Companies");
                break;
            case NavMenuItemsEnum.Stocks:
                NavigationManager.NavigateTo("/Stocks");
                break;
            case NavMenuItemsEnum.Users:
                NavigationManager.NavigateTo("/Users");
                break;
        }
    }
}