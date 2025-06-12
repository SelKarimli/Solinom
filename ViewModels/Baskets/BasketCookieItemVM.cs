namespace FinalProject.MVC.ViewModels.Baskets;

public class BasketCookieItemVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public decimal SellPrice { get; set; }
    public int Discount { get; set; }
    public int Count { get; set; }
    public int Quantity { get; set; }
    public Decimal Total { get; set; }
}
