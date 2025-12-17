using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;

        public AdminController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return View(productDto);

            try
            {
                await _productService.CreateProductAsync(productDto);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(productDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Category = product.Category
            };

            return View(productDto);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return View(productDto);

            try
            {
                await _productService.UpdateProductAsync(id, productDto);
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(productDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Dashboard");
        }
    }
}
