using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace FinalProject.MVC.Models;

public class Room:BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; }
    [Required, MaxLength(10)]
    public string RoomNumber { get; set; } // e.g., "101A", "205B
    [Required, MaxLength(500)]
    public string Description { get; set; }
    [Precision(18, 2)]
    [Range(1, 10000)]
    public decimal Price { get; set; }
    public string CoverImage { get; set; }
    public int Capacity { get; set; } = 2; // Default 2 guest
    public bool IsAvailable { get; set; } = true;
    // Navigation properties
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public string? ImageUrl { get; set; }
}
