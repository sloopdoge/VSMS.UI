namespace VSMS.Domain.Models.ViewModels;

public class StockPerformanceViewModel : StockViewModel
{
    public decimal? PreviousPrice { get; set; }

    public decimal? PriceChange => Price - PreviousPrice;
    public bool? HasIncreased => PreviousPrice.HasValue ? Price > PreviousPrice.Value : null;
}