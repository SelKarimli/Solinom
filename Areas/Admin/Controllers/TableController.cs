// Areas/Admin/Controllers/TableController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using FinalProject.MVC.DataAccess;

namespace FinalProject.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TableController : Controller
    {
        private readonly AppDbContext _context;

        public TableController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Table
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tables.ToListAsync());
        }

        // GET: Admin/Table/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // GET: Admin/Table/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Table/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TableCreateVM tableVM)
        {
            if (ModelState.IsValid)
            {
                var table = new Table
                {
                    Title = tableVM.Title,
                    Seat = tableVM.Seat,
                    Reserved = tableVM.Reserved
                };

                _context.Add(table);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tableVM);
        }

        // GET: Admin/Table/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            var tableVM = new TableCreateVM
            {
                Id = table.Id,
                Title = table.Title,
                Seat = table.Seat,
                Reserved = table.Reserved
            };

            return View(tableVM);
        }

        // POST: Admin/Table/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TableCreateVM tableVM)
        {
            if (id != tableVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var table = await _context.Tables.FindAsync(id);
                    table.Title = tableVM.Title;
                    table.Seat = tableVM.Seat;
                    table.Reserved = tableVM.Reserved;

                    _context.Update(table);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableExists(tableVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tableVM);
        }

        // GET: Admin/Table/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // POST: Admin/Table/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Tables()
        {
            var availableTables = await _context.Tables
                .Where(t => !t.Reserved) // Only show non-reserved tables
                .ToListAsync();

            return View(availableTables);
        }
        private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }
    }
}