using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Helpers;
using FinalProject.MVC.ViewModels.Baskets;
using FinalProject.MVC.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AB108Uniqlo.ViewComponents
{
    public class LayoutHeaderViewComponent(AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basket = BasketHelper.GetBasket(HttpContext.Request);

            var basketItems = await _context.Rooms
                .Where(x => basket.Select(y => y.Id).Contains(x.Id))
                .Select(x=> new BasketItemVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl,
                    SellPrice = x.Price
                })
                .ToListAsync();

            foreach (var item in basketItems)
                item.Count = basket.First(x=> x.Id == item.Id).Count;

            return View(basketItems);
        }
    }
}
