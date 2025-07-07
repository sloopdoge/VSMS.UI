using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using VSMS.Domain;

namespace VSMS.Application.Components.Pages;

public partial class IndexPage : ComponentBase
{
    [Inject] private ILogger<IndexPage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
}