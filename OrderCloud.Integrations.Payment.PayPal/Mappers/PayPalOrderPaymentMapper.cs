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
            var amount = ConvertStringAmountToDecimal(authorizedOrder.purchase_units.FirstOrDefault()?.payments.authorizations
                .FirstOrDefault()?.amount.value);
            var authorizationId = authorizedOrder.purchase_units.FirstOrDefault()?.payments.authorizations
                .FirstOrDefault()?.id;
            var ccTransaction = new CCTransactionResult
            {
                Succeeded = authorizedOrder.status.ToLowerInvariant() == "completed" && authorizationId != null,
                Amount = amount,
                TransactionID = authorizationId, // Authorization ID needed to Capture payment or Void Authorization
                ResponseCode = null,
                AuthorizationCode = null,
                AVSResponseCode = null,
                Message = null
            };
            return ccTransaction;
        }

        public CCTransactionResult MapCapturedPaymentToCCTransactionResult(PayPalOrder capturedOrder) =>
            new CCTransactionResult()
            {
                TransactionID = capturedOrder.id, // Capture ID needed to Refund payment
                Succeeded = capturedOrder.status.ToLowerInvariant() == "completed"
            };

        public CCTransactionResult MapRefundPaymentToCCTransactionResult(PayPalOrderReturn orderReturn) =>
            new CCTransactionResult
            {
                Succeeded = orderReturn.status.ToLowerInvariant() == "completed" && orderReturn.id != null,
                Amount = ConvertStringAmountToDecimal(orderReturn.amount.value),
                TransactionID = orderReturn.id,
                ResponseCode = null,
                AuthorizationCode = null,
                AVSResponseCode = null,
                Message = orderReturn.note
            };

        private decimal ConvertStringAmountToDecimal(string value) =>
            Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }
}
