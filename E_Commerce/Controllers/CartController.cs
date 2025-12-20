using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IKhaltiService _khaltiService;
        private readonly IConfiguration _configuration;

        public CartController(
        ICartService cartService,
        IOrderService orderService,
        IKhaltiService khaltiService,
        IConfiguration configuration)
        {
            _cartService = cartService;
            _orderService = orderService;
            _khaltiService = khaltiService;
            _configuration = configuration;
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
            ViewBag.KhaltiPublicKey = _configuration["Khalti:PublicKey"];
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InitiateKhaltiPayment(CheckOutDto checkoutDto)
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
               
                var order = await _orderService.CreatePendingOrderAsync(userId, checkoutDto, cart);

                
                var totalAmount = _cartService.GetCartTotal();
                var amountInPaisa = (int)(totalAmount * 100);

               
                var khaltiRequest = new KhaltiPaymentRequest
                {
                    ReturnUrl = Url.Action("KhaltiCallback", "Cart", null, Request.Scheme),
                    WebsiteUrl = $"{Request.Scheme}://{Request.Host}",
                    Amount = amountInPaisa,
                    PurchaseOrderId = order.Id.ToString(),
                    PurchaseOrderName = $"Order #{order.Id} - Guitar Purchase",
                    CustomerInfo = new CustomerInfo
                    {
                        Name = checkoutDto.ShippingName,
                        Email = checkoutDto.ShippingEmail,
                        Phone = checkoutDto.ShippingPhone
                    }
                };

               
                var khaltiResponse = await _khaltiService.InitiatePaymentAsync(khaltiRequest);

               
                await _orderService.UpdateOrderKhaltiPidxAsync(order.Id, khaltiResponse.Pidx);

               
                return Redirect(khaltiResponse.PaymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Payment initiation failed: {ex.Message}";
                return View("Checkout", checkoutDto);
            }
        }





        //[Authorize]
        //public async Task<IActionResult> KhaltiCallback(
        //         string pidx,
        //        string? txnId = null,
        //        string? transaction_id = null,
        //        string? tidx = null,
        //        int? amount = null,
        //        int? total_amount = null,
        //        string? mobile = null,
        //        string? purchase_order_id = null,
        //        string? purchase_order_name = null,
        //        string? status = null)
        //{
        //    try
        //    {

        //        Console.WriteLine($"Khalti Callback - pidx: {pidx}, status: {status}, purchase_order_id: {purchase_order_id}");

        //        if (string.IsNullOrEmpty(pidx))
        //        {
        //            TempData["Error"] = "Invalid payment response - No pidx received";
        //            return RedirectToAction("Index");
        //        }

        //        if (string.IsNullOrEmpty(purchase_order_id))
        //        {
        //            TempData["Error"] = "Invalid payment response - No order ID received";
        //            return RedirectToAction("Index");
        //        }


        //        if (!int.TryParse(purchase_order_id, out int orderId))
        //        {
        //            TempData["Error"] = "Invalid order ID";
        //            return RedirectToAction("Index");
        //        }


        //        var actualAmount = total_amount ?? amount ?? 0;


        //        var verificationResponse = await _khaltiService.VerifyPaymentAsync(pidx, actualAmount);

        //        Console.WriteLine($"Verification Status: {verificationResponse.Status}");


        //        if (verificationResponse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        //        {

        //            await _orderService.CompleteOrderAsync(
        //                orderId,
        //                verificationResponse.TransactionId,
        //                verificationResponse.Pidx);

        //            _cartService.ClearCart();

        //            TempData["Success"] = "Payment successful! Your order has been placed.";
        //            return RedirectToAction("OrderConfirmation", new { orderId = orderId });
        //        }
        //        else
        //        {
        //            TempData["Error"] = $"Payment verification failed. Status: {verificationResponse.Status}";
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Khalti Callback Error: {ex.Message}");
        //        Console.WriteLine($"Stack Trace: {ex.StackTrace}");

        //        TempData["Error"] = $"Payment verification failed: {ex.Message}";
        //        return RedirectToAction("Index");
        //    }
        //}

        [Authorize]
        public async Task<IActionResult> KhaltiCallback(
                string pidx,
                string txnId = null,
                string transaction_id = null,
                string tidx = null,
                int? amount = null,
                int? total_amount = null,
                string mobile = null,
                string purchase_order_id = null,
                string purchase_order_name = null,
                string status = null)
        {
            try
            {
                // Log for debugging
                Console.WriteLine($"=== Khalti Callback ===");
                Console.WriteLine($"pidx: {pidx}");
                Console.WriteLine($"status: {status}");
                Console.WriteLine($"purchase_order_id: {purchase_order_id}");
                Console.WriteLine($"transaction_id: {transaction_id}");
                Console.WriteLine($"amount: {total_amount ?? amount}");

                // Validate required parameters
                if (string.IsNullOrEmpty(pidx))
                {
                    TempData["Error"] = "Invalid payment response - No pidx received";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(purchase_order_id))
                {
                    TempData["Error"] = "Invalid payment response - No order ID received";
                    return RedirectToAction("Index");
                }

                // Parse order ID
                if (!int.TryParse(purchase_order_id, out int orderId))
                {
                    TempData["Error"] = "Invalid order ID format";
                    return RedirectToAction("Index");
                }

                // Get actual amount (Khalti sends in paisa, need to convert to decimal)
                var actualAmount = (total_amount ?? amount ?? 0);

                Console.WriteLine($"Verifying payment with pidx: {pidx}, amount: {actualAmount}");

                // Verify payment with Khalti API
                var verificationResponse = await _khaltiService.VerifyPaymentAsync(pidx, actualAmount);

                Console.WriteLine($"Verification Status: {verificationResponse.Status}");

                // Check if payment is completed (case-insensitive)
                if (verificationResponse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Payment verified! Completing order #{orderId}");

                    // Update order status to completed
                    await _orderService.CompleteOrderAsync(
                        orderId,
                        verificationResponse.TransactionId ?? transaction_id ?? txnId,
                        verificationResponse.Pidx ?? pidx);

                    Console.WriteLine($"Order #{orderId} marked as completed");

                    // IMPORTANT: Clear cart after successful payment
                    _cartService.ClearCart();
                    Console.WriteLine("Cart cleared");

                    TempData["Success"] = "Payment successful! Your order has been placed.";
                    return RedirectToAction("OrderConfirmation", new { orderId = orderId });
                }
                else
                {
                    Console.WriteLine($"Payment verification failed with status: {verificationResponse.Status}");
                    TempData["Error"] = $"Payment verification failed. Status: {verificationResponse.Status}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== Khalti Callback Error ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = $"Payment verification failed: {ex.Message}";
                return RedirectToAction("Index");
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
