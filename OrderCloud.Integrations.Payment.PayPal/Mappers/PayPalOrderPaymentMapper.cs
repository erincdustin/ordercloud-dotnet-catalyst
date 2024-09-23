using System;
using System.Globalization;
using System.Linq;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal.Mappers
{
    public class PayPalOrderPaymentMapper
    {
        public PurchaseUnit MapToPurchaseUnit(AuthorizeCCTransaction transaction) => new PurchaseUnit()
        {
            amount = new Amount()
            {
                currency_code = transaction.Currency,
                value = transaction.Amount.ToString(CultureInfo.InvariantCulture) ?? null
            }
        };

        public CCTransactionResult MapAuthorizedPaymentToCCTransactionResult(PayPalOrder authorizedOrder)
        {
            var amount = ConvertStringAmountToDecimal(authorizedOrder.purchase_units.FirstOrDefault()?.amount.value);
            var authorizationId = authorizedOrder.purchase_units.FirstOrDefault()?.payments.authorizations
                .FirstOrDefault()?.id;
            var ccTransaction = new CCTransactionResult
            {
                Succeeded = authorizedOrder.status.ToLowerInvariant() == "COMPLETED" && authorizationId != null,
                Amount = amount,
                TransactionID = authorizationId, // Authorization ID needed to Capture payment
                ResponseCode = null,
                AuthorizationCode = null,
                AVSResponseCode = null,
                Message = null
            };
            return ccTransaction;
        }

        public CCTransactionResult MapCapturedPaymentToCCTransactionResult(PayPalOrder capturedOrder)
        {
            var amount = ConvertStringAmountToDecimal(capturedOrder.purchase_units.FirstOrDefault()?.amount.value);
            return new CCTransactionResult()
            {
                TransactionID = capturedOrder.id, // Capture ID needed to Refund payment
                Amount = amount,
                Succeeded = capturedOrder.status.ToLowerInvariant() == "completed"
            };
        }

        public CCTransactionResult MapRefundPaymentToCCTransactionResult(PayPalOrderReturn orderReturn)
        {
            return new CCTransactionResult
            {
                Succeeded = orderReturn.status.ToLowerInvariant() == "completed",
                Amount = ConvertStringAmountToDecimal(orderReturn.amount.value),
                TransactionID = orderReturn.id,
                ResponseCode = null,
                AuthorizationCode = null,
                AVSResponseCode = null,
                Message = orderReturn.note
            };
        }

        private decimal ConvertStringAmountToDecimal(string value) =>
            Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }
}
