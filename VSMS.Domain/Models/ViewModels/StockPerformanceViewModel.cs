namespace VSMS.Domain.Models.ViewModels;

public class StockPerformanceViewModel : StockViewModel
{
    public decimal? PreviousPrice { get; set; }
    public decimal? PriceChange  { get; set; }
    public bool? HasIncreased  { get; set; }

    public void SetAdditionalInfo()
    {
        if (PreviousPrice.HasValue)
        {
            PriceChange = Price - PreviousPrice.Value;
            HasIncreased = Price > PreviousPrice.Value;
        }
        else
        {
            PriceChange = null;
            HasIncreased = null;
        }
    }

}