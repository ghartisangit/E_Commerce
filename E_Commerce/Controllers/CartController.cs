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

                Console.WriteLine($"Order #{order.Id} created");

                
                var totalAmount = _cartService.GetCartTotal();
                var amountInPaisa = (int)(totalAmount * 100);

                Console.WriteLine($"Amount: ${totalAmount} = {amountInPaisa} paisa");

              
                //var returnUrl = $"{Request.Scheme}://{Request.Host}/Cart/KhaltiCallback";   this is old one

                var returnUrl = Url.Action("KhaltiCallback", "Cart", null, Request.Scheme);
                
                var websiteUrl = $"{Request.Scheme}://{Request.Host}";

                Console.WriteLine($"Return URL: {returnUrl}");
                Console.WriteLine($"Website URL: {websiteUrl}");

              
                var khaltiRequest = new KhaltiPaymentRequest
                {
                    ReturnUrl = returnUrl,
                    WebsiteUrl = websiteUrl,
                    Amount = amountInPaisa,
                    PurchaseOrderId = order.Id.ToString(),
                    PurchaseOrderName = $"Order #{order.Id} - Purchase",
                    CustomerInfo = new CustomerInfo
                    {
                        Name = checkoutDto.ShippingName,
                        Email = checkoutDto.ShippingEmail,
                        Phone = checkoutDto.ShippingPhone
                    }
                };

                Console.WriteLine("Calling Khalti initiate API...");

              
                var khaltiResponse = await _khaltiService.InitiatePaymentAsync(khaltiRequest);

                Console.WriteLine($"Khalti Response - Pidx: {khaltiResponse.Pidx}");
                Console.WriteLine($"Payment URL: {khaltiResponse.PaymentUrl}");

             
                await _orderService.UpdateOrderKhaltiPidxAsync(order.Id, khaltiResponse.Pidx);

                Console.WriteLine($"Order #{order.Id} updated with pidx");

              
                return Redirect(khaltiResponse.PaymentUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in InitiateKhaltiPayment: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = $"Payment initiation failed: {ex.Message}";
                return View("Checkout", checkoutDto);
            }
        }



        [Authorize]
        public async Task<IActionResult> KhaltiCallback(
            string pidx = null,
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
                if (string.IsNullOrWhiteSpace(pidx))
                {
                    Console.WriteLine("ERROR: pidx is null or empty");
                    TempData["Error"] = "Invalid payment response - Missing payment identifier";
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrWhiteSpace(purchase_order_id))
                {
                    Console.WriteLine("ERROR: purchase_order_id is null or empty");
                    TempData["Error"] = "Invalid payment response - Missing order information";
                    return RedirectToAction("Index", "Home");
                }

              
                if (!int.TryParse(purchase_order_id, out int orderId))
                {
                    Console.WriteLine($"ERROR: Invalid order ID format: {purchase_order_id}");
                    TempData["Error"] = "Invalid order ID";
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine($"Processing Order ID: {orderId}");

                
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    Console.WriteLine($"ERROR: Order #{orderId} not found");
                    TempData["Error"] = "Order not found";
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine($"Order found: #{order.Id}, Status: {order.Status}");

               
                var actualAmount = total_amount ?? amount ?? 0;
                Console.WriteLine($"Payment amount: {actualAmount} paisa");

               
                Console.WriteLine($"Verifying payment with Khalti API...");
                KhaltiVerifyResponse verificationResponse;

                try
                {
                    verificationResponse = await _khaltiService.VerifyPaymentAsync(pidx, actualAmount);
                    Console.WriteLine($"Khalti Verification Status: {verificationResponse.Status}");
                    Console.WriteLine($"Khalti Transaction ID: {verificationResponse.TransactionId}");
                }
                catch (Exception verifyEx)
                {
                    Console.WriteLine($"ERROR: Khalti verification failed: {verifyEx.Message}");
                    TempData["Error"] = "Payment verification failed. Please contact support.";
                    return RedirectToAction("Index", "Home");
                }

               
                if (verificationResponse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"✓ Payment VERIFIED for Order #{orderId}");

                   
                    var transactionId = verificationResponse.TransactionId
                                     ?? transaction_id
                                     ?? txnId
                                     ?? tidx
                                     ?? "KHALTI_" + DateTime.Now.Ticks;

                    Console.WriteLine($"Transaction ID: {transactionId}");

                    
                    try
                    {
                        await _orderService.CompleteOrderAsync(orderId, transactionId, pidx);
                        Console.WriteLine($"✓ Order #{orderId} updated to Processing status");
                    }
                    catch (Exception orderEx)
                    {
                        Console.WriteLine($"ERROR: Failed to update order: {orderEx.Message}");
                        TempData["Error"] = "Failed to complete order. Please contact support.";
                        return RedirectToAction("Index", "Home");
                    }

                    
                    try
                    {
                        _cartService.ClearCart();   
                        Console.WriteLine("✓ Cart cleared");

                        //TempData["ClearCart_UserId"] = order.UserId.ToString();
                        //TempData["OrderCompleted"] = "true";

                        //Console.WriteLine($"✓ Marked cart for clearing (User ID: {order.UserId})");

                    }
                    catch (Exception cartEx)
                    {
                        Console.WriteLine($"WARNING: Failed to clear cart: {cartEx.Message}");
                        
                    }

                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("PAYMENT SUCCESSFUL - Redirecting to confirmation");
                    Console.WriteLine(new string('=', 60) + "\n");

                    TempData["Success"] = "Payment successful! Your order has been placed.";
                    return RedirectToAction("OrderConfirmation", new { orderId = orderId });
                }
                else
                {
                    Console.WriteLine($"✗ Payment NOT completed - Status: {verificationResponse.Status}");
                    TempData["Error"] = $"Payment not completed. Status: {verificationResponse.Status}";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("CRITICAL ERROR IN KHALTI CALLBACK");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
                Console.WriteLine(new string('=', 60) + "\n");

                TempData["Error"] = "An error occurred processing your payment. Please contact support.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult TestCallback()
        {
            return Content("Khalti callback route is working! ✓", "text/html");
        }
    


        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound();

            _cartService.ClearCart();

            return View(order);
        }


        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction("Index", "Home");
                }

               
                if (User.Identity.IsAuthenticated)
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (order.UserId == userId)
                    {
                        _cartService.ClearCart();
                        Console.WriteLine($"✓ Cart cleared for User #{userId}");
                    }
                }
               
                else if (TempData.ContainsKey("ClearCart_UserId"))
                {
                    var userIdFromOrder = TempData["ClearCart_UserId"]?.ToString();
                    Console.WriteLine($"✓ Cart clearing deferred for User #{userIdFromOrder}");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PaymentSuccess: {ex.Message}");
                TempData["Error"] = "Unable to load order details";
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
