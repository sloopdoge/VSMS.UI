using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Services.HttpServices;
using static VSMS.Domain.Constants.RoleNames;

namespace VSMS.Application.Components.Shared.Modals;

public partial class UserCreateModal : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Inject] private ILogger<UserCreateModal> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private UserHttpService UserHttpService { get; set; }
    
    private UserCreateViewModel CreateModel { get; set; } = new();
    private EditContext _editContext;
    private List<string> AvailableRoles { get; set; } = [User, CompanyManager, CompanyAdmin];
    
    protected override void OnInitialized()
    {
        try
        {
            _editContext = new(CreateModel);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            MudDialog.Cancel();
        }
    }
    
    private async Task HandleCompanyCreate()
    {
        try
        {
            var res = await UserHttpService.CreateUser(CreateModel);
            if (res is not null)
                MudDialog.Close(DialogResult.Ok(res));
            else
                Logger.LogError($"User {CreateModel.Email} not added");
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}