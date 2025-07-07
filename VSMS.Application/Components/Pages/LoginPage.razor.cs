using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using VSMS.Application.Identity;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Pages;

public partial class LoginPage : ComponentBase
{
    [Inject] private ILogger<LoginPage> Logger { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private ILocalStorageService LocalStorage { get; set; }
    [Inject] private AuthHttpService AuthHttpService { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    
    [Parameter] [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

    private UserLoginViewModel LoginModel { get; set; } = new();
    
    private EditContext _editContext;
    private ValidationMessageStore _errorMessageStore;
    private string? _generalErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _editContext = new(LoginModel);
            _errorMessageStore = new(_editContext);
            
            _editContext.OnFieldChanged += (sender, e) =>
            {
                _errorMessageStore.Clear(e.FieldIdentifier);
                _editContext.NotifyValidationStateChanged();
            };
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private async Task HandleLogin()
    {
        try
        {
            _errorMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            
            if (!_editContext.Validate())
                throw new("Input values are invalid");

            var authResult = await AuthHttpService.Login(LoginModel);
            if (authResult is null)
            {
                _errorMessageStore.Clear();
                _errorMessageStore.Add(new FieldIdentifier(LoginModel, nameof(LoginModel.Email)), $"There was error while making a request to a API server");
                
                _editContext.NotifyValidationStateChanged();
                return;
            }

            switch (authResult.Success)
            {
                case false:
                    _errorMessageStore.Clear();
                    _generalErrorMessage = null;

                    foreach (var error in authResult.Errors)
                    {
                        if (error.Key == "General")
                        {
                            _generalErrorMessage = error.Value;
                        }
                        else
                        {
                            _errorMessageStore.Add(new FieldIdentifier(LoginModel, error.Key), error.Value);
                        }
                    }

                    _editContext.NotifyValidationStateChanged();
                    return;

                case true:
                    _errorMessageStore.Clear();
                    _generalErrorMessage = null;

                    await ((CustomAuthStateProvider)AuthenticationStateProvider).NotifyUserAuthentication(
                        authResult.Token.Value, 
                        authResult.Token.Expires, 
                        authResult.UserProfile.Id, 
                        authResult.UserProfile.Role, 
                        authResult.UserProfile.Username, 
                        authResult.UserProfile.Email);
                 
                    NavigationManager.NavigateTo("/", forceLoad: true);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}