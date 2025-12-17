using E_Commerce.DTOs;
using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, CheckOutDto checkoutDto, List<CartItem> cartItems);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order> GetOrderByIdAsync(int orderId);
    }
}
