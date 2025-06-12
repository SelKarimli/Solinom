using System.ComponentModel.DataAnnotations;
using FinalProject.MVC.Models;
namespace FinalProject.MVC.ViewModels;
public class RoomCreateVM
{
    [Required, MaxLength(100)]
    public string? Name { get; set; }

    [Required, MaxLength(10)]
    public string? RoomNumber { get; set; }

    [Required, MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.1, double.MaxValue)]
    public decimal Price { get; set; }

    public IFormFile? CoverImageFile { get; set; } // For file upload
    public int Capacity { get; set; } = 2;
    public bool IsAvailable { get; set; } = true;

    public List<IFormFile>? AdditionalImages { get; set; } // For multiple image uploads
    public static implicit operator Room(RoomCreateVM vm)
    {
        return new Room
        {
            Price = vm.Price,
            Description = vm.Description,
            RoomNumber = vm.RoomNumber,
            Name = vm.Name,
            Capacity = vm.Capacity,
            IsAvailable = vm.IsAvailable
        };
    }
}