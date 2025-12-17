using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace E_Commerce.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
