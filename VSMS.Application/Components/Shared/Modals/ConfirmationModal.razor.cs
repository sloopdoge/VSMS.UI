using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using VSMS.Domain;

namespace VSMS.Application.Components.Shared.Modals;

public partial class ConfirmationModal : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Inject] private ILogger<CompanyViewModal> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    
    [Parameter] public string Message { get; set; } = "Are you sure you want to continue?";
    [Parameter] public string ItemName { get; set; }
}