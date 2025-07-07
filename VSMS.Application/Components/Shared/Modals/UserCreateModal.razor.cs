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
    [Inject] private UsersHttpService UsersHttpService { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    
    [Parameter] public Guid CompanyId { get; set; } = Guid.Empty;
    
    private UserCreateViewModel CreateModel { get; set; } = new();
    private EditContext _editContext;
    private List<string> AvailableRoles { get; set; } = [CompanyManager, CompanyAdmin];
    
    protected override void OnInitialized()
    {
        try
        {
            CreateModel.RoleName = CompanyManager;
            _editContext = new(CreateModel);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            MudDialog.Cancel();
        }
    }
    
    private async Task HandleUserCreate()
    {
        try
        {
            var createRes = await UsersHttpService.CreateUser(CreateModel);
            if (createRes is not null)
            {
                if (CompanyId != Guid.Empty)
                {
                    var companyAssignRes = await CompaniesHttpService.AssignUserToCompany(CompanyId, createRes.Id);
                    if (!companyAssignRes)
                        throw new Exception($"User {CreateModel.Email} was not assigned to company");
                }
                MudDialog.Close(DialogResult.Ok(createRes));
            }
            else
                throw new Exception($"User {CreateModel.Email} not added");

        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}