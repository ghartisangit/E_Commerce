using E_Commerce.DTOs;
using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(int userId, ReviewDto reviewDto);
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
        Task<double> GetProductAverageRatingAsync(int productId);
    }
}
