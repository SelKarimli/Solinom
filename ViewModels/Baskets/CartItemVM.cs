namespace FinalProject.MVC.ViewModels.Baskets
{
    public class CartItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }
}