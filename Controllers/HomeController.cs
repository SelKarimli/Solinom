using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Services.Abstracts;
using FinalProject.MVC.ViewModels;
using FinalProject.MVC.ViewModels.Baskets;
using Microsoft.AspNetCore.Mvc;
using FinalProject.MVC.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace FinalProject.MVC.Controllers;
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;

        public HomeController(AppDbContext appDbContext,ILogger<HomeController> logger,IEmailService emailService)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _emailService = emailService;
        }
    public IActionResult Gallery()
    {
        return View();

    }

    public IActionResult Index()
        {
        var rooms = _appDbContext.Rooms.ToList();

        // Pass rooms to the view using the model, not ViewData
        return View(rooms);

    }
        public IActionResult About(){return View();}
        public IActionResult Restaurant() {return View();}
        public IActionResult Rooms()
        {
        // Get all rooms from database
        var rooms = _appDbContext.Rooms.ToList();

        // Pass rooms to the view using the model, not ViewData
        return View(rooms);
        }
        [HttpGet]
        public IActionResult Contact(){return View(new ContactVM());}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    model.Name,
                    model.Email,
                    model.Phone,
                    model.Subject,
                    model.Message);

                TempData["SuccessMessage"] = "Your message has been sent successfully!";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact form");
                ModelState.AddModelError("", "An error occurred while sending your message. Please try again later.");
                return View(model);
            }
        }
        public async Task<IActionResult> Cart()
        {
            var basketItems = getBasket();

            var productIds = basketItems.Select(b => b.Id).ToList();

            var products = await _appDbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var cartViewModels = basketItems.Select(basketItem =>
            {
                var product = products.FirstOrDefault(p => p.Id == basketItem.Id);
                return new BasketItemVM
                {
                    Id = basketItem.Id,
                    Name = product?.Name ?? "Product not found",
                    SellPrice = product?.SellPrice ?? 0,
                    ImageUrl = product.CoverImage,
                    Count = basketItem.Count,
                    Total = (product?.SellPrice ?? 0) * basketItem.Count
                };
            }).ToList();

            return View(cartViewModels);
        }


        public async Task<IActionResult> Checkout(){
        var basketItems = getBasket();

        var productIds = basketItems.Select(b => b.Id).ToList();

        var products = await _appDbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var cartViewModels = basketItems.Select(basketItem =>
        {
            var product = products.FirstOrDefault(p => p.Id == basketItem.Id);
            return new BasketItemVM
            {
                Id = basketItem.Id,
                Name = product?.Name ?? "Product not found",
                SellPrice = product?.SellPrice ?? 0,
                Count = basketItem.Count,
                ImageUrl=product.CoverImage,
                Quantity = product?.Quantity ?? 0,
                Total = (product?.SellPrice ?? 0) * basketItem.Count
            };
        }).ToList();

        return View(cartViewModels);
    }
        public IActionResult PaymentSuccessful(){return View();}
        public IActionResult Products()
        {
            var products = _appDbContext.Products.ToList(); // or use a ViewModel
            return View(products);
        }
        public async Task<IActionResult> GetBasket()
        {
            return Json(getBasket());
        }
        List<BasketCookieItemVM> getBasket()
        {
            try
            {
                string? value = HttpContext.Request.Cookies["basket"];
                if (value is null) return new();
                return JsonSerializer.Deserialize<List<BasketCookieItemVM>>(value) ?? new();
            }
            catch (Exception)
            {
                return new();
            }
        }
    [HttpPost]
    public IActionResult UpdateBasket(int id, int change)
    {
        var product = _appDbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product == null || product.Quantity <= 0)
        {
            TempData["ErrorMessage"] = "Product is out of stock";
            return RedirectToAction("Products");
        }

        var basket = getBasket();
        var item = basket.FirstOrDefault(x => x.Id == id);

        if (item != null)
        {
            if (item.Count >= product.Quantity)
            {
                TempData["ErrorMessage"] = $"Only {product.Quantity} items available in stock";
                return RedirectToAction("Products");
            }
            item.Count++;
        }
        else
        {
            basket.Add(new BasketCookieItemVM
            {
                Id = id,
                Count = 1
            });
        }

        string data = JsonSerializer.Serialize(basket);
        HttpContext.Response.Cookies.Append("basket", data);

        return RedirectToAction("Cart");
    }
    [HttpPost]
    [HttpGet] // Using HttpGet to work with <a> tag
    public IActionResult DeleteBasket(int id)
    {
        var basket = getBasket();
        var item = basket.FirstOrDefault(x => x.Id == id);

        if (item != null)
        {
            basket.Remove(item);
            // Update the cookie
            string data = JsonSerializer.Serialize(basket);
            HttpContext.Response.Cookies.Append("basket", data);

            // Add a success message
            TempData["SuccessMessage"] = "Item removed from cart";
        }
        else
        {
            TempData["ErrorMessage"] = "Item not found in cart";
        }

        return RedirectToAction("Cart");
    }
    public IActionResult AddBasket(int id)
        {
            var basket = getBasket();
            var item = basket.FirstOrDefault(x => x.Id == id);
            if (item != null)
                item.Count++;
            else
            {
                basket.Add(new BasketCookieItemVM
                {
                    Id = id,
                    Count = 1
                });
            }
            string data = JsonSerializer.Serialize(basket);
            HttpContext.Response.Cookies.Append("basket", data);
            return RedirectToAction("Cart");
    }
    [HttpPost]  // This handles form submission
    [ValidateAntiForgeryToken]
    public IActionResult Create(ReservationCreateVM model)
    {
        return View("~/Views/Home/Room1.cshtml", model);
    }
    public IActionResult SearchRooms(int? capacity, decimal? minPrice, decimal? maxPrice)
    {
        var rooms = _appDbContext.Rooms.AsQueryable();

        if (capacity.HasValue)
            rooms = rooms.Where(r => r.Capacity >= capacity.Value);

        if (minPrice.HasValue)
            rooms = rooms.Where(r => r.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            rooms = rooms.Where(r => r.Price <= maxPrice.Value);

        return View("Rooms", rooms.ToList());
    }

    [HttpGet("Home/Room1/{id?}")]
    public IActionResult Room1(int id) // id is RoomId
    {
        var room = _appDbContext.Rooms.FirstOrDefault(r => r.Id == id);
        if (room == null)
        {
            return NotFound(); // or redirect to a different page
        }
        var model = new ReservationCreateVM
        {
            RoomId = room.Id,
            // Initialize other default values
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1),
            NumberOfGuests = 1
        };

        // Pass room to ViewData
        ViewData["Room"] = room;

        return View("~/Views/Home/Room1.cshtml", model);
    }
}