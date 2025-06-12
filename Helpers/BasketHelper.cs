using FinalProject.MVC.ViewModels.Baskets;
using System.Text.Json;

namespace FinalProject.MVC.Helpers
{
    public class BasketHelper
    {
        public static List<BasketCookieItemVM> GetBasket(HttpRequest request)
        {
            string? value = request.Cookies["basket"];
            if (value is null) return new();
            return JsonSerializer.Deserialize<List<BasketCookieItemVM>>(value) ?? new();
        }
    }
}
