using System.ComponentModel.DataAnnotations;

namespace VSMS.Domain.Models.ViewModels;

public class CompanyViewModel
{
    public Guid Id { get; set; }
    [Required(
        ErrorMessageResourceName = "company_title_is_required", 
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserProfileViewModel> UserProfiles { get; set; } = new();
}