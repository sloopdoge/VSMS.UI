using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using VSMS.Domain;

namespace VSMS.Application.Components.Pages.Users;

public partial class UserProfilesPage : ComponentBase
{
    [Inject] private ILogger<UserProfilesPage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
}