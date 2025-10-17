namespace VSMS.Domain.Models.ViewModels.Shared.Filters;

/// <summary>
/// Represents a filtering model for querying <see cref="CompanyViewModel"/> data.
/// </summary>
public class CompaniesFilterViewModel : BaseFilterViewModel
{
    /// <summary>
    /// Filters companies by creation date range (optional).
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Filters by partial company name match (overrides Search if both are set).
    /// </summary>
    public string? Title { get; set; }
}