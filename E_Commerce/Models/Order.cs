using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace E_Commerce.Models
{
    //public class Order
    //{
    //    public int Id { get; set; }
    //    public int UserId { get; set; }
    //    public decimal TotalAmount { get; set; }
    //    public string Status { get; set; } // "Pending", "Completed", "Cancelled"
    //    public string ShippingAddress { get; set; }
    //    public DateTime OrderDate { get; set; }

    //    // Navigation Properties
    //    public User User { get; set; }
    //    public ICollection<OrderItem> OrderItems { get; set; }
    //}


    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Pending, Completed, Failed, Cancelled
        public string? PaymentMethod { get; set; }
        public string? PaymentTransactionId { get; set; }
        public string? KhaltiPidx { get; set; } // Add this for Khalti
        public DateTime CreatedAt { get; set; }
        public string? AdminNotes { get; set; }
        // Navigation properties
        public User User { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        // Shipping details
        public string ShippingName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingEmail { get; set; }
    }


}
