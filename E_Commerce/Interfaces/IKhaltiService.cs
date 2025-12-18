using E_Commerce.Models;

namespace E_Commerce.Interfaces
{
    public interface IKhaltiService
    {
        Task<KhaltiInitiateResponse> InitiatePaymentAsync(KhaltiPaymentRequest request);
        Task<KhaltiVerifyResponse> VerifyPaymentAsync(string token, decimal amount);
      

    }
}
