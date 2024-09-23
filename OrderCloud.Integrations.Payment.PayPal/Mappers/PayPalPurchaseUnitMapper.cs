using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal.Mappers
{
    public class PayPalPurchaseUnitMapper
    {
        public PurchaseUnit MapToPurchaseUnit(AuthorizeCCTransaction transaction) => new PurchaseUnit()
        {
            amount = new Amount()
            {
                currency_code = transaction.Currency,
                value = transaction.Amount.ToString(CultureInfo.InvariantCulture) ?? null
            }
        };

        public CCTransactionResult MapOrderToCcTransactionResult(Order order, AuthorizeCCTransaction transaction) => new CCTransactionResult
        {
            // TODO: this mapper needs some help
            Succeeded = true,
            Amount = transaction.Amount,
            TransactionID = null,
            ResponseCode = order.status,
            AuthorizationCode = null,
            AVSResponseCode = null,
            Message = order.links.FirstOrDefault(l => l.rel == "approve")?.href ?? "Unable to find URL to approve order authorization"
        };
    }
}
