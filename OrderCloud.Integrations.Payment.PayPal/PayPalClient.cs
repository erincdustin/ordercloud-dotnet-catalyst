using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Flurl;
using Flurl.Http;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalClient
    {
        // https://developer.paypal.com/api/rest/authentication/
        public static async Task<string> GetAccessTokenAsync(PayPalConfig config)
        {
            var response = await config.BaseUrl
                .WithBasicAuth(config.ClientID, config.SecretKey)
                .AppendPathSegments("v1", "oauth2", "token")
                .PostUrlEncodedAsync(new
                {
                    grant_type = "client_credentials"
                });

            var tokenResponse = await response.GetJsonAsync<AuthTokenResponse>();
            return tokenResponse.access_token;
        }

        // https://developer.paypal.com/docs/api/orders/v2/#orders_create
        public static async Task<Order> CreateAuthorizedOrderAsync(PayPalConfig config, PurchaseUnit purchaseUnit)
        {
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders")
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new
                {
                    intent = "CAPTURE",
                    purchase_units = new List<PurchaseUnit>()
                    {
                        purchaseUnit
                    }
                });

            return await request.GetJsonAsync<Order>();
        }

        // https://developer.paypal.com/docs/api/orders/v2/#orders_capture
        public static async Task<Order> CapturePaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders", transaction.TransactionID, "capture")
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new { });

            return await request.GetJsonAsync<Order>();
        }



        public class AuthTokenResponse
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public int expires_in { get; set; }
            public string nonce { get; set; }
        }
    }
}
