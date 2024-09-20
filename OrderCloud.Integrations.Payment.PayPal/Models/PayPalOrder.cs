using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCloud.Integrations.Payment.PayPal.Models
{
    public class PurchaseUnit
    {
        // The merchant ID for the purchase unit.
        public string reference_id { get; set; }
        public Amount amount { get; set; }
    }

    public class PaymentSource
    {
        public Card card { get; set; }
        public PayPal paypal { get; set; }
    }

    public class Card
    {
        public string name { get; set; }
        public string number { get; set; }
        public string security_code { get; set; }
        public string expiry { get; set; }
    }

    public class PayPal
    {
        public Name name { get; set; }
        public string email_address { get; set; }
    }

    public class Name
    {
        public string given_name { get; set; }
        public string surname { get; set; }
    }

    public class Amount
    {
        // The three-character ISO-4217 currency code.
        public string currency_code { get; set; }
        // The total amount charged to the payee by the payer. For refunds, represents the amount that the payee refunds to the original payer. Maximum length is 10 characters, which includes:
        // 
        // Seven digits before the decimal point.
        // The decimal point.
        // Two digits after the decimal point
        public string value { get; set; }
    }

    public class Order
    {
        public string id { get; set; }
        public string status { get; set; }
        public List<OrderLink> links { get; set; }
    }

    public class OrderLink
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }
}
