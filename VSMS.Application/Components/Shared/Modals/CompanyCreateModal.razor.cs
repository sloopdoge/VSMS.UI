using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Application.Components.Shared.Modals;

public partial class CompanyCreateModal : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Inject] private ILogger<CompanyCreateModal> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private CompaniesHttpService CompaniesHttpService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    private CompanyViewModel CreateModel { get; set; } = new();
    private EditContext _editContext;

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

    private async Task HandleAddCompany()
    {
        try
        {
            var res = await CompaniesHttpService.CreateCompany(CreateModel);
            if (res is not null)
                MudDialog.Close(DialogResult.Ok(res));
            else
                Logger.LogError($"Company {CreateModel.Title} not added");
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}