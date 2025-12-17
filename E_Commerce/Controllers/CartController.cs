using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["Success"] = "Item removed from cart";
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            ViewBag.Total = _cartService.GetCartTotal();
            ViewBag.CartItems = cart;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckOutDto checkoutDto)
        {
            if (!ModelState.IsValid)
                return View("Checkout", checkoutDto);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = _cartService.GetCart();

            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, checkoutDto, cart);
                _cartService.ClearCart();
                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Checkout", checkoutDto);
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }
    }
}
