namespace VSMS.Domain.Models.ViewModels.Shared.Filters;

/// <summary>
/// Represents a filtering model for querying <see cref="StockViewModel"/> data.
/// </summary>
public class StocksFilterViewModel : BaseFilterViewModel
{
    /// <summary>
    /// Optional filter by multiple company identifiers.
    /// </summary>
    public List<Guid>? CompanyIds { get; set; }

    /// <summary>
    /// Optional filter by multiple stock symbols (case-insensitive).
    /// </summary>
    public List<string>? Symbols { get; set; }

    /// <summary>
    /// Optional lower bound for stock price.
    /// </summary>
    public decimal? PriceFrom { get; set; }

    /// <summary>
    /// Optional upper bound for stock price.
    /// </summary>
    public decimal? PriceTo { get; set; }

    /// <summary>
    /// Optional filter by stock title (partial match).
    /// </summary>
    public string? Title { get; set; }
}