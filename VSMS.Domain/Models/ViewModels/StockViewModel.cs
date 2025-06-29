namespace VSMS.Domain.Models.ViewModels;

public class StockViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CompanyId { get; set; }
}