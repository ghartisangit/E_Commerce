using E_Commerce.Interfaces;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace E_Commerce.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string search)
        {
            var products = string.IsNullOrEmpty(search)
                ? await _productService.GetAllProductsAsync()
                : await _productService.SearchProductsAsync(search);

            ViewBag.SearchTerm = search;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
