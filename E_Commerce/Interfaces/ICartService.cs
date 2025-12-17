using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface ICartService
    {
        List<CartItem> GetCart();
        void AddToCart(int productId, string productName, decimal price, string imageUrl);
        void UpdateQuantity(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        decimal GetCartTotal();
    }
}
