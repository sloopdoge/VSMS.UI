using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using VSMS.Domain;
using VSMS.Domain.Models.ViewModels;
using VSMS.Infrastructure.Helpers;
using static VSMS.Domain.Constants.RoleNames;

namespace VSMS.Application.Components.Pages.Users;

public partial class UserProfilePage : ComponentBase
{
    [Inject] private ILogger<UserProfilePage> Logger { get; set; }
    [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
    [Inject] private TimeZoneHelper TimeZoneHelper { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    
    [Parameter] public Guid UserId { get; set; }
    
    private bool IsLoading { get; set; } = true;
    private UserProfileViewModel UserProfile { get; set; } = new();
    private string UserRole { get; set; } = User;
    private List<string> AdditionalInfoRoles { get; set; } = [Admin, CompanyAdmin];
}