using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LeKatsuMNL.Services
{
    public class PayMongoService : IPayMongoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public PayMongoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Get from environment variables loaded by DotNetEnv in Program.cs
            _secretKey = Environment.GetEnvironmentVariable("SECRET_KEYPAY") ?? "";
            
            if (string.IsNullOrEmpty(_secretKey))
            {
                // Fallback or log error
                Console.WriteLine("Warning: PayMongo SECRET_KEYPAY is not set.");
            }

            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_secretKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            _httpClient.BaseAddress = new Uri("https://api.paymongo.com/v1/");
        }

        public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(int orderId, decimal amount, string description, string successUrl, string cancelUrl)
        {
            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        cancel_url = cancelUrl,
                        description = description,
                        line_items = new[]
                        {
                            new
                            {
                                amount = (int)(amount * 100), // convert to cents
                                currency = "PHP",
                                name = $"Order #{orderId}",
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "card", "gcash", "paymaya" },
                        reference_number = orderId.ToString(),
                        success_url = successUrl
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("checkout_sessions", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var data = doc.RootElement.GetProperty("data");
                return new CheckoutSessionResult
                {
                    SessionId = data.GetProperty("id").GetString(),
                    CheckoutUrl = data.GetProperty("attributes").GetProperty("checkout_url").GetString()
                };
            }

            return new CheckoutSessionResult { Error = "ERROR: " + responseBody };
        }

        public async Task<bool> IsPaymentSuccessfulAsync(string sessionId)
        {
            var response = await _httpClient.GetAsync($"checkout_sessions/{sessionId}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var attributes = doc.RootElement.GetProperty("data").GetProperty("attributes");
                
                var status = attributes.GetProperty("status").GetString();
                if (status == "expired") return false;

                var payments = attributes.GetProperty("payments");
                if (payments.GetArrayLength() > 0)
                {
                    var pStatus = payments[0].GetProperty("attributes").GetProperty("status").GetString();
                    return pStatus == "paid";
                }
            }

            return false;
        }

        public async Task<(string Method, string PaymentId)> GetPaymentDetailsAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"checkout_sessions/{sessionId}?expand=payments");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseBody);
                    var attributes = doc.RootElement.GetProperty("data").GetProperty("attributes");
                
                var payments = attributes.GetProperty("payments");
                if (payments.GetArrayLength() > 0)
                {
                    var payment = payments[0];
                    var paymentId = payment.GetProperty("id").GetString();
                    var pAttributes = payment.GetProperty("attributes");
                    
                    // The specific method can be in several places depending on the session type
                    string method = "PayMongo";
                    if (pAttributes.TryGetProperty("payment_method_used", out var pmu))
                    {
                        method = pmu.GetString() ?? "PayMongo";
                    }
                    else if (pAttributes.TryGetProperty("source", out var src) && src.TryGetProperty("type", out var typ))
                    {
                        method = typ.GetString() ?? "PayMongo";
                    }
                    else if (pAttributes.TryGetProperty("payment_method_type", out var pmt))
                    {
                        method = pmt.GetString() ?? "PayMongo";
                    }

                    return (method, paymentId);
                }
            }
            }
            catch { }

            return ("PayMongo", "Confirmed-" + sessionId.Substring(Math.Max(0, sessionId.Length - 8)));
        }

        public async Task<string> GetCheckoutSessionStatusAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"checkout_sessions/{sessionId}?expand=payments");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var attributes = doc.RootElement.GetProperty("data").GetProperty("attributes");
                    var status = attributes.GetProperty("status").GetString() ?? "unknown";

                    // Robust check: sessions can be 'active' but have a successful payment
                    if (status != "paid")
                    {
                        if (attributes.TryGetProperty("payments", out var payments) && payments.ValueKind == JsonValueKind.Array && payments.GetArrayLength() > 0)
                        {
                            foreach (var payment in payments.EnumerateArray())
                            {
                                if (payment.TryGetProperty("attributes", out var pAttrs) && 
                                    pAttrs.TryGetProperty("status", out var pStatus) && 
                                    pStatus.GetString() == "paid")
                                {
                                    return "paid";
                                }
                            }
                        }
                    }
                    return status;
                }
            }
            catch (Exception ex) 
            {
                return $"error: {ex.Message}";
            }
            return "error";
        }
    }
}
