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


        public async Task<List<Order>> GetAllOrdersAsync(OrderFilterDto filter)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(filter.SearchTerm) ||
                    o.ShippingName.Contains(filter.SearchTerm) ||
                    o.ShippingEmail.Contains(filter.SearchTerm) ||
                    o.ShippingPhone.Contains(filter.SearchTerm));
            }

            // Pagination
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return orders;
        }

        public async Task<Order> GetOrderDetailsByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return false;

            order.Status = dto.Status;
            order.AdminNotes = dto.AdminNotes;
            //order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var stats = new OrderStatisticsDto
            {
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing"),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed"),
                CancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled"),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount),
                TodayRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= today && o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount),
                MonthRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= firstDayOfMonth && o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount)
            };

            return stats;
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = "Cancelled";
            order.AdminNotes = $"Cancelled: {reason}";
            //order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalOrdersCountAsync(OrderFilterDto filter)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(filter.SearchTerm) ||
                    o.ShippingName.Contains(filter.SearchTerm) ||
                    o.ShippingEmail.Contains(filter.SearchTerm));
            }

            return await query.CountAsync();
        }

    }
}
