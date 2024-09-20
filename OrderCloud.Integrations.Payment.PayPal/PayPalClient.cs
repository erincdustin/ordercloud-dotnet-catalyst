using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Flurl;
using Flurl.Http;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalClient
    {
        protected static IFlurlRequest BuildClient(PayPalConfig config) =>
            config.BaseUrl.WithBasicAuth(config.ClientID, config.SecretKey);

        public async Task<string> GetAccessTokenAsync(PayPalConfig config)
        {
            var response = await BuildClient(config)
                .AppendPathSegments("v1", "oauth2", "token")
                .PostUrlEncodedAsync(new
                {
                    grant_type = "client_credentials"
                });

            var tokenResponse = await response.GetJsonAsync<AuthTokenResponse>();
            return tokenResponse.access_token;
        }

        public async Task<Order> CreateAuthorizedOrderAsync(PayPalConfig config, string token)
        {
            // https://developer.paypal.com/docs/api/orders/v2/
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(new
                {
                    intent = "AUTHORIZE", // "CAPTURE" 
                    purchase_units = new List<PurchaseUnit>()
                    {
                        new PurchaseUnit()
                        {
                            reference_id = null,
                            amount = new Amount()
                            {
                                currency_code = "USD",
                                value = "321.45",
                            }
                        }
                    },
                    //payment_source = new PaymentSource()
                    //{
                    //    paypal = new Models.PayPal()
                    //    {
                    //        email_address = "dummy@dummy.com", // this pre-populates the email field in the paypal window
                    //        name = new Name()
                    //        {
                    //            given_name = "Fake",
                    //            surname = "Person"
                    //        }
                    //    }
                    //}
                });

            var order = await request.GetJsonAsync<Order>();

            var approveUrl = order.links.FirstOrDefault(l => l.rel == "approve");

            var approvalRequest = approveUrl?.rel.GetJsonAsync();
            //var getOrderData = await config.BaseUrl
            //    .AppendPathSegments("v2", "checkout", "orders", order.id)
            //    .WithOAuthBearerToken(token)
            //    .GetJsonAsync();
            
            return order;
        }

        // https://developer.paypal.com/docs/api/orders/v2/#orders_authorize
        public async Task<Order> CapturePaymentAsync(PayPalConfig config, string token, string orderId, string ppRequestId)
        {
            try
            {
                var request = await config.BaseUrl
                    .AppendPathSegments("v2", "checkout", "orders", orderId, "capture")
                    .WithHeader("PayPal-Request-Id", ppRequestId) // I assume this comes back from the browser
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(new
                    {
                        payment_source = new PaymentSource()
                        {
                            paypal = new Models.PayPal()
                            {
                                email_address = "dummy@dummy.com",
                                name = new Name()
                                {
                                    given_name = "Fake",
                                    surname = "Person"
                                }
                            }
                        }

                    });

                var order = await request.GetJsonAsync<Order>();
                return order;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
