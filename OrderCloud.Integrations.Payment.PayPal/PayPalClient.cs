using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<string> GetUserInfo(PayPalConfig config, string token)
        {
            try
            {
                var response = await config.BaseUrl
                    .AppendPathSegments("v1", "identity", "openidconnect", "userinfo")
                    .WithOAuthBearerToken(token)
                    .SetQueryParams(new { schema = "openid" })
                    .GetJsonAsync<dynamic>();

                Console.WriteLine(response);
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        public async Task<Order> CreateOrderAsync(PayPalConfig config, string token)
        {
            // https://developer.paypal.com/docs/api/orders/v2/
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(new
                {
                    intent = "CAPTURE", // "AUTHORIZE" ???
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

            var getOrderData = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders", order.id)
                .WithOAuthBearerToken(token)
                .GetJsonAsync();

            Console.WriteLine(getOrderData);

            return order;
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
