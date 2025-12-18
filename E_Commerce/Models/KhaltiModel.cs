namespace E_Commerce.Models
{
    public class KhaltiPaymentRequest
    {
        public string ReturnUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public decimal Amount { get; set; } // Amount in paisa (NPR * 100)
        public string PurchaseOrderId { get; set; }
        public string PurchaseOrderName { get; set; }
        public CustomerInfo CustomerInfo { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class KhaltiInitiateResponse
    {
        public string? Pidx { get; set; }
        public string PaymentUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class KhaltiVerifyResponse
    {
        public string? Pidx { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string? TransactionId { get; set; }
        public decimal Fee { get; set; }
        public bool Refunded { get; set; }
    }
}
