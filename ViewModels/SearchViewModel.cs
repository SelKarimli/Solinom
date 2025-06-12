using FinalProject.MVC.Models;
using FinalProject.MVC.Services;
namespace FinalProject.MVC.ViewModels;

public class SearchViewModel
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public SearchType SearchType { get; set; } = SearchType.All;
    public SearchResults Results { get; set; } = new SearchResults();
    public IEnumerable<Room> Rooms { get; set; } = new List<Room>();
    public IEnumerable<Table> Tables { get; set; } = new List<Table>();
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
}
