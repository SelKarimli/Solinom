using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinalProject.MVC.DataAccess;
using Microsoft.EntityFrameworkCore;
using FinalProject.MVC.Extensions;

namespace FinalProject.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoomController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public RoomController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    public async Task<IActionResult> Index(int? page = 1, int? take = 4)
    {
        if (!page.HasValue) page = 1;
        if (!take.HasValue) take = 4;
        var query = _context.Rooms.AsQueryable();
        var data = await query.Skip(take.Value * (page.Value - 1)).Take(take.Value).ToListAsync();
        int total = await query.CountAsync();
        ViewBag.Rooms = new PaginationItemsVM(total, take.Value, page.Value);
        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(RoomCreateVM vm)
    {
        if (vm.CoverImageFile != null)
        {
            if (!vm.CoverImageFile.IsValidType("image"))
                ModelState.AddModelError("File", "File must be an image");
            if (!vm.CoverImageFile.IsValidSize(100000))
                ModelState.AddModelError("File", "File must be less than 100000kb");
        }
        if (vm.AdditionalImages != null && vm.AdditionalImages.Any())
        {
            if (!vm.AdditionalImages.All(x => x.IsValidType("image")))
            {
                string fileNames = string.Join(',', vm.AdditionalImages.Where(x => !x.IsValidType("image")).Select(x => x.FileName));
                ModelState.AddModelError("OtherFiles", fileNames + " is (are) not an image");
            }
            if (!vm.AdditionalImages.All(x => x.IsValidSize(400)))
            {
                string fileNames = string.Join(',', vm.AdditionalImages.Where(x => !x.IsValidSize(400)).Select(x => x.FileName));
                ModelState.AddModelError("OtherFiles", fileNames + " is (are) bigger than 400kb");
            }
        }
        if (!ModelState.IsValid)
        {
            return View(vm);
        }
        Room room = vm;
        room.CoverImage = await vm.CoverImageFile!.UploadAsync(_env.WebRootPath, "imgs", "rooms");
        room.Images = vm.AdditionalImages?.Select(x => new RoomImage{ImageUrl = x.UploadAsync(_env.WebRootPath, "imgs", "rooms").Result}).ToList();
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return NotFound();

        var room = await _context.Rooms
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (room == null) return NotFound();

        var vm = new RoomUpdateVM
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Name = room.Name,
            Description = room.Description,
            Price = room.Price,
            IsAvailable = room.IsAvailable,
            Capacity = room.Capacity,
            FileUrl = room.CoverImage,
            OtherFilesUrls = room.Images.Select(i => i.ImageUrl).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Update(int? id, RoomUpdateVM vm)
    {
        if (id is null || id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var room = await _context.Rooms
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (room is null) return NotFound();

        // Update basic properties
        room.Name = vm.Name;
        room.RoomNumber = vm.RoomNumber;
        room.Description = vm.Description;
        room.Price = vm.Price;
        room.Capacity = vm.Capacity;
        room.IsAvailable = vm.IsAvailable;

        // Handle cover image update
        if (vm.CoverImageFile != null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(room.CoverImage))
            {
                var oldImagePath = Path.Combine(_env.WebRootPath, "imgs", "rooms", room.CoverImage);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Upload new image
            room.CoverImage = await vm.CoverImageFile.UploadAsync(_env.WebRootPath, "imgs", "rooms");
        }

        // Handle image deletions
        if (vm.ImagesToDelete != null && vm.ImagesToDelete.Any())
        {
            foreach (var imageUrl in vm.ImagesToDelete)
            {
                var imageToDelete = room.Images.FirstOrDefault(i => i.ImageUrl == imageUrl);
                if (imageToDelete != null)
                {
                    // Delete from file system
                    var imagePath = Path.Combine(_env.WebRootPath, "imgs", "rooms", imageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    // Remove from database
                    _context.RoomImage.Remove(imageToDelete);
                }
            }
        }

        // Handle new image uploads
        if (vm.AdditionalImages != null && vm.AdditionalImages.Any())
        {
            foreach (var file in vm.AdditionalImages)
            {
                var imageUrl = await file.UploadAsync(_env.WebRootPath, "imgs", "rooms");
                room.Images.Add(new RoomImage { ImageUrl = imageUrl });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return BadRequest();

        var room = await _context.Rooms
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (room is null) return NotFound();

        // Delete cover image
        if (!string.IsNullOrEmpty(room.CoverImage))
        {
            var coverImagePath = Path.Combine(_env.WebRootPath, "imgs", "rooms", room.CoverImage);
            if (System.IO.File.Exists(coverImagePath))
            {
                System.IO.File.Delete(coverImagePath);
            }
        }

        // Delete additional images
        foreach (var image in room.Images)
        {
            var imagePath = Path.Combine(_env.WebRootPath, "imgs", "rooms", image.ImageUrl);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}