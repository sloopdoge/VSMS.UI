using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using VSMS.Domain;

namespace VSMS.Application.Components.Pages.Users;

public partial class Users : ComponentBase
{
    [Inject] private ILogger<Users> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
}