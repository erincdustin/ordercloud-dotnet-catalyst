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

        #region Step 1: Create order with Authorize intent
        // https://developer.paypal.com/docs/api/orders/v2/#orders_create
        public static async Task<PayPalOrder> CreateAuthorizedOrderAsync(PayPalConfig config, PurchaseUnit purchaseUnit, AuthorizeCCTransaction transaction)
        {
            var paymentSource = new PaymentSource();
            if (transaction.CardDetails != null)
            {
                if (transaction.CardDetails.SavedCardID != null)
                {
                    paymentSource.token = new PaymentToken()
                    {
                        id = transaction.CardDetails.SavedCardID,
                        type = "BILLING_AGREEMENT"
                    };
                } else {
                    paymentSource.card = new Card
                    {
                        name = transaction.CardDetails.CardHolderName,
                        last_digits = transaction.CardDetails.NumberLast4Digits,
                        expiry = $"{transaction.CardDetails.ExpirationMonth}/{transaction.CardDetails.ExpirationYear}",
                        brand = transaction.CardDetails.CardType
                    };
                }

            }
            return await BuildClient(config)
                .AppendPathSegments("v2", "checkout", "orders")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new
                {
                    intent = "AUTHORIZE",
                    purchase_units = new List<PurchaseUnit>()
                        {
                            purchaseUnit
                        },
                    payment_source = paymentSource
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
                .PostJsonAsync(new { })
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
        public static async Task<IFlurlResponse> VoidPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            return await BuildClient(config)
                .AppendPathSegments("v2", "payments", "authorizations", transaction.TransactionID, "void")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .PostJsonAsync(new { });
        }

        // https://developer.paypal.com/docs/api/payments/v2/#captures_refund
        public static async Task<PayPalOrderReturn> RefundPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            // get capture details to get the currency
            var captureDetails = await BuildClient(config)
                .AppendPathSegments("v2", "payments", "captures", transaction.TransactionID)
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .GetJsonAsync<PayPalCapture>();

            return await BuildClient(config)
                .AppendPathSegments("v2", "payments", "captures", transaction.TransactionID, "refund")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .WithHeader("Prefer", "return=representation")
                .PostJsonAsync(new
                {
                    amount = new
                    {
                        value = transaction.Amount.ToString(),
                        captureDetails.amount.currency_code
                    },
                    captureDetails.invoice_id
                }).ReceiveJson<PayPalOrderReturn>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#payment-tokens_create
        public static async Task<PayPalPaymentToken> CreatePaymentTokenAsync(PayPalConfig config, PCISafeCardDetails card, PaymentSystemCustomer customer)
        {
            var response =  await BuildClient(config)
                .AppendPathSegments("v3", "vault", "payment-tokens")
                .PostJsonAsync(new
                {
                    payment_source = new PaymentSource()
                    {
                        token = new PaymentToken()
                        {
                            id = card.Token,
                            type = "SETUP_TOKEN"
                        }
                    }
                });

            return await response.GetJsonAsync<PayPalPaymentToken>();
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
                .GetJsonAsync<PayPalPaymentToken>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#payment-tokens_deletes
        public static async Task DeletePaymentTokenAsync(PayPalConfig config, string tokenID)
        {
            await BuildClient(config)
                .AppendPathSegments("v3", "vault", "payment-tokens", tokenID)
                .DeleteAsync();
        }
    }
}
