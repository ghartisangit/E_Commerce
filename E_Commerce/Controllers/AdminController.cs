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
        private readonly IOrderService _orderService;
        public AdminController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var products = await _productService.GetAllProductsAsync();
            var statistics = await _orderService.GetOrderStatisticsAsync();
            var recentOrders = await _orderService.GetRecentOrdersAsync(5);

            ViewBag.Statistics = statistics;
            ViewBag.RecentOrders = recentOrders;
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
                ImageUrl = product.ImageUrl
              
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

        public async Task<IActionResult> Orders(OrderFilterDto filter)
        {
            filter ??= new OrderFilterDto();

            var orders = await _orderService.GetAllOrdersAsync(filter);
            var totalCount = await _orderService.GetTotalOrdersCountAsync(filter);

            ViewBag.Filter = filter;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return View(orders);
        }

      
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderService.GetOrderDetailsByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction("Orders");
            }

            return View(order);
        }

       
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data";
                return RedirectToAction("OrderDetails", new { id = dto.OrderId });
            }

            var result = await _orderService.UpdateOrderStatusAsync(dto);

            if (result)
            {
                TempData["Success"] = "Order status updated successfully";
            }
            else
            {
                TempData["Error"] = "Failed to update order status";
            }

            return RedirectToAction("OrderDetails", new { id = dto.OrderId });
        }

      
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "Please provide a reason for cancellation";
                return RedirectToAction("OrderDetails", new { id = orderId });
            }

            var result = await _orderService.CancelOrderAsync(orderId, reason);

            if (result)
            {
                TempData["Success"] = "Order cancelled successfully";
            }
            else
            {
                TempData["Error"] = "Failed to cancel order";
            }

            return RedirectToAction("OrderDetails", new { id = orderId });
        }

       
        [HttpPost]
        public async Task<JsonResult> QuickStatusUpdate(int orderId, string status)
        {
            try
            {
                var dto = new UpdateOrderStatusDto
                {
                    OrderId = orderId,
                    Status = status,
                    AdminNotes = $"Status updated to {status} by admin"
                };

                var result = await _orderService.UpdateOrderStatusAsync(dto);

                return Json(new { success = result, message = result ? "Status updated" : "Update failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
