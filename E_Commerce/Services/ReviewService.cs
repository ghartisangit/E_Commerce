using E_Commerce.Data;
using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Review> AddReviewAsync(int userId, ReviewDto reviewDto)
        {
            var review = new Review
            {
                ProductId = reviewDto.ProductId,
                UserId = userId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }
    }
}
