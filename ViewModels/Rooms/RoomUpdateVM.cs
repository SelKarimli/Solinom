using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.ViewModels;

public class RoomUpdateVM
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(10)]
    public string RoomNumber { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Capacity { get; set; }
    public string? FileUrl { get; set; }
    public IFormFile? CoverImageFile { get; set; }
    public IEnumerable<string>? OtherFilesUrls { get; set; }
    public List<IFormFile>? AdditionalImages { get; set; }

    // Add this property for images to delete
    public bool IsAvailable { get; set; } = true;
    public List<string>? ImagesToDelete { get; set; }
}

