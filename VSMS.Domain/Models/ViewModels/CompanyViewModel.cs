namespace VSMS.Domain.Models.ViewModels;

public class CompanyViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserProfileViewModel> UserProfiles { get; set; }
}