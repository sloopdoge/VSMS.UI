namespace VSMS.Domain.Models.ViewModels.Shared.Filters;

public class BaseFilterViewModel : IFilterViewModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; } = true;
    public string? Search { get; set; }
}