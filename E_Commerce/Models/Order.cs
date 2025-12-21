using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace E_Commerce.Models
{
  

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } 
        public string? PaymentMethod { get; set; }
        public string? PaymentTransactionId { get; set; }
        public string? KhaltiPidx { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string? AdminNotes { get; set; }
        
        public User User { get; set; }
        public List<OrderItem> OrderItems { get; set; }

      
        public string ShippingName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingEmail { get; set; }
    }


}
