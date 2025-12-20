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


        Task<List<Order>> GetAllOrdersAsync(OrderFilterDto filter);
        Task<Order> GetOrderDetailsByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();
        Task<List<Order>> GetRecentOrdersAsync(int count = 10);
        Task<bool> CancelOrderAsync(int orderId, string reason);
        Task<int> GetTotalOrdersCountAsync(OrderFilterDto filter);
    }
}
