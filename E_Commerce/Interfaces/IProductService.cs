using E_Commerce.DTOs;
using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductDto productDto);
        Task<Product> UpdateProductAsync(int id, ProductDto productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    }
}
