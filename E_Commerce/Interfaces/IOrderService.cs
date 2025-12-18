using E_Commerce.DTOs;
using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreatePendingOrderAsync(int userId, CheckOutDto checkoutDto, List<CartItem> cart);
        Task UpdateOrderKhaltiPidxAsync(int orderId, string pidx);
        Task CompleteOrderAsync(int orderId, string transactionId, string pidx);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetUserOrdersAsync(int userId);
    }
}
