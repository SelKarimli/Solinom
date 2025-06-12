using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/[controller]/[action]")]
public class AdminReservationController : Controller
{
    private readonly AppDbContext _context;

    private readonly ILogger<AdminReservationController> _logger;

    public AdminReservationController(AppDbContext context, ILogger<AdminReservationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> PendingReservations(int page = 1, int pageSize = 10)
    {
        try
        {
            var reservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Pending)
                .Include(r => r.Room)      
                .Include(r => r.User)   
                .OrderByDescending(r => r.CheckInDate)  
                .AsNoTracking()             
                .ToListAsync();

            var model = new PendingReservationsVM
            {
                Reservations = reservations
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending reservations");
            TempData["ErrorMessage"] = "An error occurred while loading reservations. Please try again later.";
            return RedirectToAction("Dashboard", "Admin"); // Redirect to safe location
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending reservations can be approved";
                return RedirectToAction(nameof(PendingReservations));
            }

            reservation.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation approved successfully";
            return RedirectToAction(nameof(PendingReservations));
        }
        catch (Exception ex)
        {
            // Log the error
            TempData["ErrorMessage"] = "An error occurred while approving the reservation";
            return RedirectToAction(nameof(PendingReservations));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
            reservation.Status = ReservationStatus.Rejected;

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            TempData["SuccessMessage"] = "Reservation rejected successfully";
            return RedirectToAction(nameof(PendingReservations));
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }
        catch (Exception ex)
        {
            // Log the error
            return StatusCode(500, "An error occurred while retrieving reservation details");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason = null)
    {
        try
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully";
            return RedirectToAction(nameof(PendingReservations));
        }
        catch (Exception ex)
        {
            // Log the error
            TempData["ErrorMessage"] = "An error occurred while cancelling the reservation";
            return RedirectToAction(nameof(PendingReservations));
        }
    }
}