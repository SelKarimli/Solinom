// ISearchService.cs
using FinalProject.MVC.DataAccess;
using FinalProject.MVC.Models;
using Microsoft.EntityFrameworkCore;
// SearchService.cs
public class SearchService : ISearchService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SearchService> _logger;

    public SearchService(AppDbContext context, ILogger<SearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SearchResults> SearchAsync(string query, int? categoryId = null, SearchType? searchType = null)
    {
        try
        {
            var results = new SearchResults();

            if (string.IsNullOrWhiteSpace(query))
            {
                return results;
            }

            var searchTerm = $"%{query.ToLower()}%";

            if (searchType == null || searchType == SearchType.All || searchType == SearchType.Rooms)
            {
                results.Rooms = await SearchRooms(searchTerm, categoryId);
            }

            if (searchType == null || searchType == SearchType.All || searchType == SearchType.Tables)
            {
                results.Tables = await SearchTables(searchTerm, categoryId);
            }

            if (searchType == null || searchType == SearchType.All || searchType == SearchType.Products)
            {
                results.Products = await SearchProducts(searchTerm, categoryId);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching");
            throw;
        }
    }

    public async Task<IEnumerable<Room>> SearchRooms(string searchTerm, int? categoryId)
    {
        var query = _context.Rooms
            .Where(r => EF.Functions.Like(r.Name.ToLower(), searchTerm) ||
                        EF.Functions.Like(r.Description.ToLower(), searchTerm));

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.Id == categoryId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Table>> SearchTables(string searchTerm, int? categoryId)
    {
        var query = _context.Tables
            .Where(t => EF.Functions.Like(t.Title.ToLower(), searchTerm));

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.Id == categoryId.Value);
        }

        return await query.ToListAsync();
    }

    private async Task<IEnumerable<Product>> SearchProducts(string searchTerm, int? categoryId)
    {
        var query = _context.Products
            .Where(p => EF.Functions.Like(p.Name.ToLower(), searchTerm) ||
                        EF.Functions.Like(p.Description.ToLower(), searchTerm));

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.Id == categoryId.Value);
        }

        return await query.ToListAsync();
    }
}
public interface ISearchService
{
    Task<SearchResults> SearchAsync(string query, int? categoryId = null, SearchType? searchType = null);
}

public enum SearchType
{
    All,
    Rooms,
    Tables,
    Products
}

public class SearchResults
{
    public IEnumerable<Room> Rooms { get; set; } = new List<Room>();
    public IEnumerable<Table> Tables { get; set; } = new List<Table>();
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
}