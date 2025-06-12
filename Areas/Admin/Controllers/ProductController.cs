using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Extensions;
using FinalProject.MVC.Helpers;
using FinalProject.MVC.Models;
using FinalProject.MVC.ViewModels;
using FinalProject.MVC.Views.Account.Enums;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Net.Mime;

namespace FinalProject.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.Product)]
    public class ProductController(IWebHostEnvironment _env,AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index(int? page = 1, int? take = 4)
        {
            if (!page.HasValue) page = 1;
            if (!take.HasValue) take = 4;
            var query = _context.Products.AsQueryable();
            var data = await query.Skip(take.Value*(page.Value-1)).Take(take.Value).ToListAsync();
            int total = await query.CountAsync();
            ViewBag.PaginationItems = new PaginationItemsVM(total, take.Value, page.Value);
            return View(data);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateVM vm)
        {
            if (vm.File != null) 
            {
                if (!vm.File.IsValidType("image"))
                    ModelState.AddModelError("File", "File must be an image");
                if (!vm.File.IsValidSize(100000))
                    ModelState.AddModelError("File", "File must be less than 100000kb");
            }
            if (vm.OtherFiles != null && vm.OtherFiles.Any())
            {
                if (!vm.OtherFiles.All(x=> x.IsValidType("image")))
                {
                    string fileNames = string.Join(',', vm.OtherFiles.Where(x => !x.IsValidType("image")).Select(x=> x.FileName));
                    ModelState.AddModelError("OtherFiles", fileNames + " is (are) not an image");
                }
                if (!vm.OtherFiles.All(x=> x.IsValidSize(100000)))
                {
                    string fileNames = string.Join(',', vm.OtherFiles.Where(x => !x.IsValidSize(100000)).Select(x => x.FileName));
                    ModelState.AddModelError("OtherFiles", fileNames + " is (are) bigger than 100000kb");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            Product product = vm;
            product.CoverImage = await vm.File!.UploadAsync(_env.WebRootPath,"imgs","products");
            product.Images = vm.OtherFiles?.Select(x => new ProductImage
            {
                ImageUrl = x.UploadAsync(_env.WebRootPath, "imgs", "products").Result 
            }).ToList();
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new ProductUpdateVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CostPrice = product.CostPrice,
                SellPrice = product.SellPrice,
                Discount = product.Discount,
                Quantity = product.Quantity,
                FileUrl = product.CoverImage,
                OtherFilesUrls = product.Images.Select(i => i.ImageUrl).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, ProductUpdateVM vm)
        {
            if (id is null || id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            // Update basic properties
            product.Name = vm.Name;
            product.Description = vm.Description;
            product.CostPrice = vm.CostPrice;
            product.SellPrice = vm.SellPrice;
            product.Discount = vm.Discount;
            product.Quantity = vm.Quantity;

            // Handle cover image update
            if (vm.File != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.CoverImage))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "imgs", "products", product.CoverImage);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Upload new image
                product.CoverImage = await vm.File.UploadAsync(_env.WebRootPath, "imgs", "products");
            }

            // Handle image deletions
            if (vm.ImagesToDelete != null && vm.ImagesToDelete.Any())
            {
                foreach (var imageUrl in vm.ImagesToDelete)
                {
                    var imageToDelete = product.Images.FirstOrDefault(i => i.ImageUrl == imageUrl);
                    if (imageToDelete != null)
                    {
                        // Delete from file system
                        var imagePath = Path.Combine(_env.WebRootPath, "imgs", "products", imageUrl);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }

                        // Remove from database
                        _context.ProductImages.Remove(imageToDelete);
                    }
                }
            }

            // Handle new image uploads
            if (vm.OtherFiles != null && vm.OtherFiles.Any())
            {
                foreach (var file in vm.OtherFiles)
                {
                    var imageUrl = await file.UploadAsync(_env.WebRootPath, "imgs", "products");
                    product.Images.Add(new ProductImage { ImageUrl = imageUrl });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            // Delete cover image
            if (!string.IsNullOrEmpty(product.CoverImage))
            {
                var coverImagePath = Path.Combine(_env.WebRootPath, "imgs", "products", product.CoverImage);
                if (System.IO.File.Exists(coverImagePath))
                {
                    System.IO.File.Delete(coverImagePath);
                }
            }

            // Delete additional images
            foreach (var image in product.Images)
            {
                var imagePath = Path.Combine(_env.WebRootPath, "imgs", "products", image.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
