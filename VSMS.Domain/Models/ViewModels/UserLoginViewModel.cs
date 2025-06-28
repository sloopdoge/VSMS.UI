using System.ComponentModel.DataAnnotations;

namespace VSMS.Domain.Models.ViewModels;

public class UserLoginViewModel
{
    [Required(
        ErrorMessageResourceName = "user_login_email_required", 
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [EmailAddress(
        ErrorMessageResourceName = "user_login_email_not_valid",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public required string Email { get; set; }
    [Required(
        ErrorMessageResourceName = "user_login_password_required", 
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    public bool UseLongLivedToken { get; set; } = false;
}