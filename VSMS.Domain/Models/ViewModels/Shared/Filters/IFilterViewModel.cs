namespace VSMS.Domain.Models.ViewModels.Shared.Filters;

/// <summary>
/// Defines a base contract for all filter models that support pagination and sorting.
/// </summary>
public interface IFilterViewModel
{
    /// <summary>
    /// Gets or sets the page number (starting from 1).
    /// </summary>
    int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the field name by which to sort.
    /// </summary>
    string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (true = ascending, false = descending).
    /// </summary>
    bool SortAscending { get; set; }

    /// <summary>
    /// Gets or sets a general search query (text-based).
    /// </summary>
    string? Search { get; set; }
}