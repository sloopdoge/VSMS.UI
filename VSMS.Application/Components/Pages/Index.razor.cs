using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using VSMS.Domain;

namespace VSMS.Application.Components.Pages;

public partial class Index : ComponentBase
{
    [Inject] private ILogger<Index> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
}