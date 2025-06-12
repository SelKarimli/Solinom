using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinalProject.MVC.DataAccess;

namespace FinalProject.MVC.Controllers;

public class ReservationController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ReservationController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Create(int roomId)
    {
        var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            return NotFound();
        }

        var model = new ReservationCreateVM
        {
            RoomId = room.Id,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1),
            NumberOfGuests = 1,
            RoomName = room.Name,
            Price = room.Price,
            Capacity = room.Capacity
        };

        ViewData["Room"] = room;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateVM model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Room"] = _context.Rooms.FirstOrDefault(r => r.Id == model.RoomId);
            return View(model);
        }

        // Check room availability
        if (!await IsRoomAvailable(model.RoomId, model.CheckInDate, model.CheckOutDate))
        {
            ModelState.AddModelError("", "The room is not available for the selected dates");
            ViewData["Room"] = _context.Rooms.FirstOrDefault(r => r.Id == model.RoomId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var reservation = new Reservation
        {
            RoomId = model.RoomId,
            UserId = user.Id,
            CheckInDate = model.CheckInDate,
            CheckOutDate = model.CheckOutDate,
            NumberOfGuests = model.NumberOfGuests,
            SpecialRequests = model.SpecialRequests,
            Status = ReservationStatus.Pending,
            TotalPrice = CalculateTotalPrice(model.RoomId, model.CheckInDate, model.CheckOutDate)
        };

        try
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation created successfully!";
            return RedirectToAction("Confirmation", new { id = reservation.Id });
        }
        catch (Exception ex)
        {
            // Log the error
            ModelState.AddModelError("", "An error occurred while creating the reservation.");
            ViewData["Room"] = _context.Rooms.FirstOrDefault(r => r.Id == model.RoomId);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Confirmation(int id)
    {
        var reservation = _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefault(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    private async Task<bool> IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut)
    {
        return !await _context.Reservations
            .Where(r => r.RoomId == roomId)
            .Where(r => r.Status != ReservationStatus.Rejected && r.Status != ReservationStatus.Rejected)
            .AnyAsync(r =>
                (checkIn >= r.CheckInDate && checkIn < r.CheckOutDate) ||
                (checkOut > r.CheckInDate && checkOut <= r.CheckOutDate) ||
                (checkIn <= r.CheckInDate && checkOut >= r.CheckOutDate));
    }

    private decimal CalculateTotalPrice(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var room = _context.Rooms.Find(roomId);
        var nights = (checkOut - checkIn).Days;
        return room.Price * nights;
    }
}