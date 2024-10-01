using System.Linq;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal.Mappers
{
    public class PayPalPaymentTokensMapper
    {
        public PCISafeCardDetails MapPaymentTokenToPCISafeCardDetails(PayPalPaymentToken paymentToken)
        {
            // Credit Card
            if (paymentToken.payment_source.card != null)
            {
                var expiryYear = paymentToken.payment_source.card?.expiry.Split('-').First();
                var expiryMonth = paymentToken.payment_source.card?.expiry.Split('-').Last();
                return new PCISafeCardDetails
                {
                    SavedCardID = paymentToken.id,
                    Token = null,
                    CardHolderName = paymentToken.payment_source.card?.name,
                    NumberLast4Digits = paymentToken.payment_source.card?.last_digits,
                    ExpirationMonth = expiryMonth,
                    ExpirationYear = expiryYear,
                    CardType = paymentToken.payment_source.card?.brand
                };
            }
            else // PayPal
            {
                var nameObj = paymentToken.payment_source.paypal?.name;
                return new PCISafeCardDetails
                {
                    SavedCardID = paymentToken.id,
                    Token = null,
                    CardHolderName = $"{nameObj.given_name} {nameObj.surname}",
                    NumberLast4Digits = null,
                    ExpirationMonth = null,
                    ExpirationYear = null,
                    CardType = "PayPal"
                };
            }
        }

        public CardCreatedResponse MapPaymentTokenToCardCreatedResponse(PayPalPaymentToken paymentToken) =>
            new CardCreatedResponse
            {
                Card = new PCISafeCardDetails()
                {
                    SavedCardID = paymentToken.id,
                    Token = null,
                    CardHolderName = paymentToken.payment_source.card?.name,
                    NumberLast4Digits = paymentToken.payment_source.card?.last_digits,
                    ExpirationMonth = paymentToken.payment_source.card?.expiry.Split('-').Last(),
                    ExpirationYear = paymentToken.payment_source.card?.expiry.Split('-').First(),
                    CardType = paymentToken.payment_source.card?.brand
                },
                CustomerID = paymentToken.customer.id
            };
    }
}
