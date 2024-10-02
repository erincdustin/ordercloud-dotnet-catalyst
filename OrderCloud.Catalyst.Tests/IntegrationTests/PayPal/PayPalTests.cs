using System.Linq;
using NUnit.Framework;
using OrderCloud.Integrations.Payment.PayPal;
using System.Collections.Generic;

namespace OrderCloud.Catalyst.Tests.IntegrationTests
{
    public class PayPalTests
    {
        [Test]
        public void ShouldThrowErrorIfDefaultConfigMissingFields()
        {
            var config = new PayPalConfig();
            var ex = Assert.Throws<IntegrationMissingConfigsException>(() =>
                new PayPalService(config)
            );
            var data = (IntegrationMissingConfigs)ex.Errors[0].Data;
            Assert.AreEqual(data.ServiceName, "PayPal");
            Assert.True(data.MissingFieldNames.All(new List<string>{ "SecretKey", "BaseUrl", "ClientID" }.Contains));
        }
    }
}
