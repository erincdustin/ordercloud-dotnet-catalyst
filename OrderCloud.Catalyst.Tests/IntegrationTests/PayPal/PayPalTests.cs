using System;
using AutoFixture;
using NUnit.Framework;
using OrderCloud.Integrations.Payment.PayPal;
using System.Threading.Tasks;

namespace OrderCloud.Catalyst.Tests.IntegrationTests
{
    public class PayPalTests
    {
        private static Fixture _fixture = new Fixture();
        
        //[Test]
        //public void ShouldThrowErrorIfDefaultConfigMissingFields()
        //{
        //    var config = new PayPalConfig();
        //    var ex = Assert.Throws<IntegrationMissingConfigsException>(() =>
        //        new PayPalService(config)
        //    );
        //    var data = (IntegrationMissingConfigs)ex.Errors[0].Data;
        //    Assert.AreEqual(data.ServiceName, "PayPal");
        //    Assert.AreEqual(new List<string> { "SecretKey" }, data.MissingFieldNames);
        //}

        [Test]
        public async Task CanGetUserInfo()
        {
            var ppClient = new PayPalClient();
            var config = new PayPalConfig()
            {
                BaseUrl = "https://api-m.sandbox.paypal.com",
                ClientID = "",
                SecretKey = ""
            };

            var token = await ppClient.GetAccessTokenAsync(config);
            await new PayPalClient().CreateOrderAsync(config, token);
        }    
    }
}
