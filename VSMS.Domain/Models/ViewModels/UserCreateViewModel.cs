using System.ComponentModel.DataAnnotations;
using VSMS.Domain.Constants;

namespace VSMS.Domain.Models.ViewModels;

public class UserCreateViewModel
{
    [Required]
    public string Username {get; set;}
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public string RoleName { get; set; } = RoleNames.User;
}