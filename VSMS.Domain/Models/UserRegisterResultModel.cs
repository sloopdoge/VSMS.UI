using VSMS.Domain.Models.ViewModels;

namespace VSMS.Domain.Models;

public class UserRegisterResultModel
{
    public bool Success { get; set; }
    public UserProfileViewModel? UserProfile { get; set; }
    public List<string>? Errors { get; set; }
}