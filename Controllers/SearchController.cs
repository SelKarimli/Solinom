using Microsoft.AspNetCore.Mvc;
using FinalProject.MVC.Services;
using FinalProject.MVC.Models;
using System.Threading.Tasks;
using FinalProject.MVC.ViewModels;

namespace FinalProject.MVC.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        // GET: /search?q=query
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    ViewBag.Message = "Please enter a search term";
                    return View(new SearchViewModel());
                }

                var results = await _searchService.SearchAsync(q, null, null);

                var viewModel = new SearchViewModel
                {
                    Query = q,
                    Rooms = results.Rooms,
                    Tables = results.Tables,
                    Products = results.Products
                };

                if (!viewModel.Rooms.Any() && !viewModel.Tables.Any() && !viewModel.Products.Any())
                {
                    ViewBag.Message = $"No results found for '{q}'";
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search");
                ViewBag.Message = "An error occurred during your search";
                return View(new SearchViewModel());
            }
        }

        // GET: /search/suggestions?q=query
        [HttpGet("suggestions")]
        public async Task<IActionResult> Suggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new List<string>());
            }

            var results = await _searchService.SearchAsync(q, null, null);

            var suggestions = results.Rooms.Select(r => r.Name)
                                .Concat(results.Tables.Select(t => t.Title))
                                .Concat(results.Products.Select(p => p.Name))
                                .Take(5)
                                .ToList();

            return Json(suggestions);
        }
    }
}