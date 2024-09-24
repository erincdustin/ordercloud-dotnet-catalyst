using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalClient
    {
        protected static IFlurlRequest BuildClient(PayPalConfig config) => config.BaseUrl.WithBasicAuth(config.ClientID, config.SecretKey);
        #region Step 1: Create order with Authorize intent. Return approve URL to client
        // https://developer.paypal.com/docs/api/orders/v2/#orders_create
        public static async Task<PayPalOrder> CreateAuthorizedOrderAsync(PayPalConfig config, PurchaseUnit purchaseUnit, string requestId)
        {
            return await BuildClient(config)
                .AppendPathSegments("v2", "checkout", "orders")
                .WithHeader("PayPal-Request-Id", requestId)
                .PostJsonAsync(new
                {
                    intent = "AUTHORIZE",
                    purchase_units = new List<PurchaseUnit>()
                        {
                            purchaseUnit
                        }
                })
                .ReceiveJson<PayPalOrder>();
        }
        #endregion

        #region Step 2: Authorize the order
        // https://developer.paypal.com/docs/api/orders/v2/#orders_authorize
        public static async Task<PayPalOrder> AuthorizePaymentForOrderAsync(PayPalConfig config, AuthorizeCCTransaction transaction)
        {
            return await BuildClient(config)
                .AppendPathSegments("v2", "checkout", "orders", transaction.OrderID, "authorize")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new
                    { })
                .ReceiveJson<PayPalOrder>();
        }
        #endregion

        #region Step 3: Capture the order
        // https://developer.paypal.com/docs/api/payments/v2/#authorizations_capture
        public static async Task<PayPalOrder> CapturePaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            return await BuildClient(config)
                .AppendPathSegments("v2", "payments", "authorizations", transaction.TransactionID, "capture")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new { })
                .ReceiveJson<PayPalOrder>();
        }
        #endregion

        // https://developer.paypal.com/docs/api/payments/v2/#authorizations_void
        public static async Task VoidPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            await BuildClient(config)
                .AppendPathSegments("v2", "payments", "authorizations", transaction.TransactionID, "void")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new { });
        }

        // https://developer.paypal.com/docs/api/payments/v2/#captures_refund
        public static async Task<PayPalOrderReturn> RefundPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            return await BuildClient(config)
                .AppendPathSegments("v2", "payments", "captures", transaction.TransactionID, "refund")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new { }).ReceiveJson<PayPalOrderReturn>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#customer_payment-tokens_get
        public static async Task<PaymentTokenResponse> ListPaymentTokensAsync(PayPalConfig config, string customerID)
        {
            return await BuildClient(config)
                .AppendPathSegments("v3", "vault", "payment-tokens")
                .SetQueryParam("customer_id", customerID)
                .GetJsonAsync<PaymentTokenResponse>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#payment-tokens_get
        public static async Task<PayPalPaymentToken> GetPaymentTokenAsync(PayPalConfig config, string tokenID)
        {
            return await BuildClient(config)
                .AppendPathSegments("v3", "vault", "payment-tokens", tokenID)
                .WithHeader("PayPal-Request-ID", "")
                .GetJsonAsync<PayPalPaymentToken>();
        }
    }
}
