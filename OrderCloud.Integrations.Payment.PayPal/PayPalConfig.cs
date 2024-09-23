using OrderCloud.Catalyst;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalConfig : OCIntegrationConfig
    {
        public override string ServiceName { get; } = "PayPal";
        [RequiredIntegrationField]
        public string BaseUrl { get; set; }
        [RequiredIntegrationField]
        public string ClientID {get; set; }
        [RequiredIntegrationField]
        public string SecretKey { get; set; }
        public string Token { get; set; }
    }
}
