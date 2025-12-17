using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;
        private readonly ICartService _cartService;

        public ProductController(
            IProductService productService,
            IReviewService reviewService,
            ICartService cartService)
        {
            _productService = productService;
            _reviewService = reviewService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            var reviews = await _reviewService.GetProductReviewsAsync(id);
            var avgRating = await _reviewService.GetProductAverageRatingAsync(id);

            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;
            ViewBag.IsLoggedIn = User.Identity.IsAuthenticated;

            return View(product);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, decimal price, string imageUrl)
        {
            _cartService.AddToCart(productId, productName, price, imageUrl);
            TempData["Success"] = "Product added to cart!";
            return RedirectToAction("Details", new { id = productId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(ReviewDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid review data";
                return RedirectToAction("Details", new { id = reviewDto.ProductId });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _reviewService.AddReviewAsync(userId, reviewDto);

            TempData["Success"] = "Review added successfully!";
            return RedirectToAction("Details", new { id = reviewDto.ProductId });
        }
    }
}
