namespace E_Commerce.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // "Pending", "Completed", "Cancelled"
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
