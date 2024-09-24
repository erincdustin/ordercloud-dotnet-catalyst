using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Mappers;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalService : OCIntegrationService, ICreditCardProcessor, ICreditCardSaver
    {
        #region ICreditCardProcessor

        public Task<string> GetIFrameCredentialAsync(OCIntegrationConfig overrideConfig = null)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> GetAuthenticatedResponseAsync(AuthorizeCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            var token = await PayPalClient.GetAccessTokenAsync(config);
            var purchaseUnitMapper = new PayPalOrderPaymentMapper();
            var purchaseUnit = purchaseUnitMapper.MapToPurchaseUnit(transaction);
            var requestID = Guid.NewGuid().ToString();
            var order = await PayPalClient.CreateAuthorizedOrderAsync(config, purchaseUnit, requestID);
            return new AuthenticationResponse()
            {
                Token = token,
                Url = order.links.FirstOrDefault(l => l.rel == "approve")?.href,
                TransactionID = order.id,
                RequestID = requestID
            };
        }

        public async Task<CCTransactionResult> AuthorizeOnlyAsync(AuthorizeCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            // AuthorizeCCTransaction.OrderID represents PayPal OrderID, NOT OrderCloud OrderID
            var authorizedPaymentForOrder = await PayPalClient.AuthorizePaymentForOrderAsync(config, transaction);
            var ccTransactionMapper = new PayPalOrderPaymentMapper();

            return ccTransactionMapper.MapAuthorizedPaymentToCCTransactionResult(authorizedPaymentForOrder);
            // CCTransactionResult.TransactionID represents the PayPal Authorization ID
        }

        public async Task<CCTransactionResult> CapturePriorAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            // FollowUpCCTransaction.TransactionID represents the PayPal Authorization ID
            var capturedPaymentForOrder = await PayPalClient.CapturePaymentAsync(config, transaction);
            var ccTransactionMapper = new PayPalOrderPaymentMapper();
            return ccTransactionMapper.MapCapturedPaymentToCCTransactionResult(capturedPaymentForOrder);
            // CCTransactionResult.TransactionID represents the PayPal Capture ID
        }

        public async Task<CCTransactionResult> VoidAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            // FollowUpCCTransaction.TransactionID represents the PayPal Authorization ID
            await PayPalClient.VoidPaymentAsync(config, transaction);
            return new CCTransactionResult() // TODO: fix this
            {
                Amount = transaction.Amount,
                Succeeded = true
            };
        }

        public async Task<CCTransactionResult> RefundCaptureAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            // FollowUpCCTransaction.TransactionID represents the PayPal Capture ID
            var orderReturn = await PayPalClient.RefundPaymentAsync(config, transaction);
            var ccTransactionMapper = new PayPalOrderPaymentMapper();
            return ccTransactionMapper.MapRefundPaymentToCCTransactionResult(orderReturn);
            // CCTransactionResult.TransactionID represents PayPal Refund ID
        }

        #endregion

        #region ICreditCardSaver
        public PayPalService(OCIntegrationConfig defaultConfig) : base(defaultConfig)
        {
        }

        public async Task<List<PCISafeCardDetails>> ListSavedCardsAsync(string customerID, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            var paymentTokenResponse = await PayPalClient.ListPaymentTokensAsync(config, customerID);
            var listOfCardDetails = new List<PCISafeCardDetails>();
            var cardDetailsMapper = new PayPalPaymentTokensMapper();
            foreach (var paymentToken in paymentTokenResponse.payment_tokens)
            {
                var mappedDetails = cardDetailsMapper.MapPaymentTokenToPCISafeCardDetails(paymentToken);
                listOfCardDetails.Add(mappedDetails);
            }

            return listOfCardDetails;
        }

        public async Task<PCISafeCardDetails> GetSavedCardAsync(string customerID, string cardID, OCIntegrationConfig overrideConfig = null)
        {
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            var paymentToken = await PayPalClient.GetPaymentTokenAsync(config, cardID);
            var cardDetailsMapper = new PayPalPaymentTokensMapper();
            return cardDetailsMapper.MapPaymentTokenToPCISafeCardDetails(paymentToken);
        }

        public Task<CardCreatedResponse> CreateSavedCardAsync(PaymentSystemCustomer customer, PCISafeCardDetails card,
            OCIntegrationConfig overrideConfig = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSavedCardAsync(string customerID, string cardID, OCIntegrationConfig overrideConfig = null)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
