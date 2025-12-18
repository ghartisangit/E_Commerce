using E_Commerce.Data;
using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

      


        public async Task<Order> CreatePendingOrderAsync(int userId, CheckOutDto checkoutDto, List<CartItem> cart)
        {
            var order = new Order
            {
                UserId = userId,
                TotalAmount = cart.Sum(c => c.Price * c.Quantity),
                Status = "Pending",
                PaymentMethod = "Khalti",
                KhaltiPidx = null,
                CreatedAt = DateTime.UtcNow,
                ShippingName = checkoutDto.ShippingName,
                ShippingAddress = checkoutDto.ShippingAddress,
                ShippingCity = checkoutDto.ShippingCity,
                ShippingPhone = checkoutDto.ShippingPhone,
                ShippingEmail = checkoutDto.ShippingEmail,
                OrderItems = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateOrderKhaltiPidxAsync(int orderId, string pidx)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.KhaltiPidx = pidx;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CompleteOrderAsync(int orderId, string transactionId, string pidx)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = "Completed";
                order.PaymentTransactionId = transactionId;
                order.KhaltiPidx = pidx;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
               .ThenInclude(oi => oi.Product)           
                .Include(o => o.User)                   
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.PaymentMethod ??= "Not Set";
                order.PaymentTransactionId ??= "Pending";
                order.KhaltiPidx ??= "N/A";
            }
            return order;
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
         .Include(o => o.OrderItems)
         .ThenInclude(oi => oi.Product)
         .Include(o => o.User)
         .Where(o => o.UserId == userId)
         .OrderByDescending(o => o.CreatedAt)
         .ToListAsync();

            // Handle null values for each order
            foreach (var order in orders)
            {
                order.PaymentMethod ??= "Not Set";
                order.PaymentTransactionId ??= "Pending";
                order.KhaltiPidx ??= "N/A";
            }

            return orders;
        }

    }
}
