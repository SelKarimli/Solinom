// TableController.cs
using Microsoft.AspNetCore.Mvc;
using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels.Products;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using FinalProject.MVC.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject.MVC.Controllers
{
    public class TableController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public TableController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var availableTables = await _context.Tables
                .Where(t => !t.Reserved) // Only show non-reserved tables
                .ToListAsync();

            return View(availableTables);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int tableId, DateTime reservationDate)
        {
            if (reservationDate < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Reservation date must be in the future";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var table = await _context.Tables.FindAsync(tableId);

            if (table == null)
            {
                TempData["ErrorMessage"] = "Table not found";
                return RedirectToAction("Index");
            }

            if (table.Reserved)
            {
                TempData["ErrorMessage"] = "Table is already reserved";
                return RedirectToAction("Index");
            }

            try
            {
                var reservation = new TableReservation
                {
                    TableId = tableId,
                    UserId = userId,
                    ReservationDate = reservationDate,
                    DepositAmount = 20.00m,
                    IsConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TableReservations.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction("Checkout", new { reservationId = reservation.Id });
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while processing your reservation";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public async Task<IActionResult> Checkout(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.TableReservations
                .Include(r => r.Table)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null || reservation.IsConfirmed)
            {
                return NotFound();
            }

            return View("ConfirmReservation", reservation);
        }
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            var reservation = await _context.TableReservations
                .Include(r => r.Table)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.IsConfirmed = true;
            await _context.SaveChangesAsync();

            return View(reservation);
        }
    }
}