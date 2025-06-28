namespace VSMS.Domain.Models.ViewModels;

public class UserRegisterViewModel
{
    public required string Username {get; set;}
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public required string Email { get; set; }
    public string PhoneNumber { get; set; }
    public required string Password { get; set; }
}