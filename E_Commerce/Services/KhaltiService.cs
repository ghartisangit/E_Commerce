using E_Commerce.Interfaces;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
namespace E_Commerce.Services
{
    public class KhaltiService : IKhaltiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private const string KhaltiApiUrl = "https://a.khalti.com/api/v2";

        public KhaltiService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _secretKey = configuration["Khalti:SecretKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Key {_secretKey}");
        }

        public async Task<KhaltiInitiateResponse> InitiatePaymentAsync(KhaltiPaymentRequest request)
        {
            var payload = new
            {
                return_url = request.ReturnUrl,
                website_url = request.WebsiteUrl,
                amount = (int)request.Amount, 
                purchase_order_id = request.PurchaseOrderId,
                purchase_order_name = request.PurchaseOrderName,
                customer_info = new
                {
                    name = request.CustomerInfo.Name,
                    email = request.CustomerInfo.Email,
                    phone = request.CustomerInfo.Phone
                }
            };

            var jsonContent = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{KhaltiApiUrl}/epayment/initiate/", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Khalti API Error: {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

            return new KhaltiInitiateResponse
            {
                Pidx = result.pidx,
                PaymentUrl = result.payment_url,
                ExpiresAt = result.expires_at,
                ExpiresIn = result.expires_in
            };
        }

        public async Task<KhaltiVerifyResponse> VerifyPaymentAsync(string pidx, decimal amount)
        {
            try
            {
                var payload = new { pidx = pidx };
                var jsonContent = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"Calling Khalti Verify API with pidx: {pidx}");

                var response = await _httpClient.PostAsync($"{KhaltiApiUrl}/epayment/lookup/", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Khalti API Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Khalti API Error: {response.StatusCode}");
                    throw new Exception($"Khalti Verification Error: {responseContent}");
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                return new KhaltiVerifyResponse
                {
                    Pidx = result.pidx?.ToString() ?? pidx,
                    TotalAmount = (decimal)(result.total_amount ?? 0) / 100m,
                    Status = result.status?.ToString() ?? "Unknown",
                    TransactionId = result.transaction_id?.ToString() ?? "",
                    Fee = (decimal)(result.fee ?? 0) / 100m,
                    Refunded = result.refunded ?? false
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Khalti Verify Exception: {ex.Message}");
                throw;
            }
        }
    }
}
