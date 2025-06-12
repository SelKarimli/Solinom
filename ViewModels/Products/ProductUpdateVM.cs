using System.ComponentModel.DataAnnotations;

namespace FinalProject.MVC.ViewModels
{
    public class ProductUpdateVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal SellPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, 100)]
        public int Discount { get; set; }

        public string? FileUrl { get; set; }
        public IFormFile? File { get; set; }
        public IEnumerable<string>? OtherFilesUrls { get; set; }
        public ICollection<IFormFile>? OtherFiles { get; set; }

        // Add this property for images to delete
        public List<string>? ImagesToDelete { get; set; }
    }
}