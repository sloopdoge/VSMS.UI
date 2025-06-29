using VSMS.Domain.Models.ViewModels;

namespace VSMS.Domain.Models;

public class LoginResultModel
{
    public bool Success { get; set; }
    public TokenModel? Token { get; set; }
    public UserProfileViewModel? UserProfile { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}