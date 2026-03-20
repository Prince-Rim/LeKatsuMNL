using System.Threading.Tasks;

namespace LeKatsuMNL.Services
{
    public class CheckoutSessionResult
    {
        public string CheckoutUrl { get; set; }
        public string SessionId { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }

    public interface IPayMongoService
    {
        /// <summary>
        /// Creates a PayMongo Checkout Session for an order.
        /// </summary>
        /// <returns>A result containing the checkout URL and Session ID.</returns>
        Task<CheckoutSessionResult> CreateCheckoutSessionAsync(int orderId, decimal amount, string description, string successUrl, string cancelUrl);

        /// <summary>
        /// Retrieves the status of a Payment Intent or Checkout Session.
        /// </summary>
        Task<bool> IsPaymentSuccessfulAsync(string sessionId);

        /// <summary>
        /// Retrieves the specific payment method used (e.g. gcash, paymaya) and the Payment ID.
        /// </summary>
        Task<(string Method, string PaymentId)> GetPaymentDetailsAsync(string sessionId);

        /// <summary>
        /// Retrieves the overall checkout session status (e.g. "paid", "expired").
        /// </summary>
        Task<string> GetCheckoutSessionStatusAsync(string sessionId);

        /// <summary>
        /// Retrieves the full checkout session details.
        /// </summary>
        Task<CheckoutSessionResult> GetCheckoutSessionAsync(string sessionId);
    }
}
