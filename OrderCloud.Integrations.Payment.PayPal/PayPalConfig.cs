using OrderCloud.Catalyst;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalConfig : OCIntegrationConfig
    {
        public override string ServiceName { get; } = "PayPal";
        public string BaseUrl { get; set; }
        public string ClientID {get; set; }
        public string SecretKey { get; set; }
    }
}
