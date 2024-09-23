using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Mappers;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalService : OCIntegrationService, ICreditCardProcessor, ICreditCardSaver
    {
        #region ICreditCardProcessor

        public async Task<string> GetIFrameCredentialAsync(OCIntegrationConfig overrideConfig = null)
        {
            // this can authenticate paypal
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            var token = await PayPalClient.GetAccessTokenAsync(config);
            return token;
        }

        public async Task<CCTransactionResult> AuthorizeOnlyAsync(AuthorizeCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            // this can create the order, with AUTHORIZE as the intent
            // must include the token in the overrideConfig
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);
            var purchaseUnitMapper = new PayPalPurchaseUnitMapper();
            var purchaseUnit = purchaseUnitMapper.MapToPurchaseUnit(transaction);

            var order = await PayPalClient.CreateAuthorizedOrderAsync(config, purchaseUnit);
            return purchaseUnitMapper.MapOrderToCcTransactionResult(order, transaction);
        }

        public async Task<CCTransactionResult> CapturePriorAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            // this can capture the order
            var config = ValidateConfig<PayPalConfig>(overrideConfig ?? _defaultConfig);

            var response = await PayPalClient.CapturePaymentAsync(config, transaction);
            return new CCTransactionResult()
            {
                Amount = transaction.Amount,
                Succeeded = response.status.ToLower() == "completed"
            };
        }

        public Task<CCTransactionResult> VoidAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            throw new NotImplementedException();
        }

        public Task<CCTransactionResult> RefundCaptureAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null)
        {
            throw new NotImplementedException();
        }

        //public async Task<CCTransactionResult> VoidAuthorizationAsync(FollowUpCCTransaction transaction,
        //    OCIntegrationConfig configOverride = null)
        //{
        //    var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //    var paymentIntentMapper = new StripePaymentIntentMapper();
        //    var cancelPaymentIntentOptions = paymentIntentMapper.MapPaymentIntentCancelOptions(transaction);
        //    var canceledPaymentIntent = await PayPalClient.CancelPaymentIntentAsync(transaction.TransactionID, cancelPaymentIntentOptions, config);
        //    return paymentIntentMapper.MapPaymentIntentCancelResponse(canceledPaymentIntent);
        //}

        //public async Task<CCTransactionResult> RefundCaptureAsync(FollowUpCCTransaction transaction,
        //    OCIntegrationConfig configOverride = null)
        //{
        //    var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //    var refundMapper = new StripeRefundMapper();
        //    var refundCreateOptions = refundMapper.MapRefundCreateOptions(transaction);
        //    var refund = await PayPalClient.CreateRefundAsync(refundCreateOptions, config);
        //    return refundMapper.MapRefundCreateResponse(refund);
        //}
        //#endregion

        //#region ICreditCardSaver

        //public async Task<CardCreatedResponse> CreateSavedCardAsync(PaymentSystemCustomer customer,
        //    PCISafeCardDetails card, OCIntegrationConfig configOverride = null)
        //{
        //    var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //    var paymentMethodMapper = new StripePaymentMethodMapper();

        //    if (!customer.CustomerAlreadyExists)
        //    {
        //        var stripeCustomerOptions = StripeCustomerCreateMapper.MapCustomerOptions(customer);
        //        var stripeCustomer = await PayPalClient.CreateCustomerAsync(stripeCustomerOptions, config);
        //        customer.ID = stripeCustomer.Id;
        //    }

        //    var paymentMethodCreateOptions = paymentMethodMapper.MapPaymentMethodCreateOptions(customer.ID, card);
        //    var paymentMethod = await PayPalClient.CreatePaymentMethodAsync(paymentMethodCreateOptions, config);
        //    var paymentMethodAttachOptions = paymentMethodMapper.MapPaymentMethodAttachOptions(customer.ID);
        //    paymentMethod = await PayPalClient.AttachPaymentMethodToCustomerAsync(paymentMethod.Id, paymentMethodAttachOptions, config);
        //    return paymentMethodMapper.MapPaymentMethodCreateResponse(customer.ID, paymentMethod);
        //}

        //public async Task<List<PCISafeCardDetails>> ListSavedCardsAsync(string customerID,
        //    OCIntegrationConfig configOverride = null)
        //{
        //    {
        //        var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //        var paymentMethodMapper = new StripePaymentMethodMapper();
        //        var listPaymentMethodsOptions = paymentMethodMapper.MapPaymentMethodListOptions(customerID);
        //        var paymentMethodList = await PayPalClient.ListPaymentMethodsAsync(listPaymentMethodsOptions, config);
        //        return paymentMethodMapper.MapStripePaymentMethodListResponse(paymentMethodList);
        //    }
        //}

        //public async Task<PCISafeCardDetails> GetSavedCardAsync(string customerID, string paymentMethodID,
        //    OCIntegrationConfig configOverride = null)
        //{
        //    var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //    var cardMapper = new StripeCardMapper();
        //    var paymentMethod = await PayPalClient.RetrievePaymentMethodAsync(paymentMethodID, config);
        //    return cardMapper.MapStripeCardGetResponse(paymentMethod);
        //}

        //public async Task DeleteSavedCardAsync(string customerID, string paymentMethodID, OCIntegrationConfig configOverride = null)
        //{
        //    var config = ValidateConfig<PayPalConfig>(configOverride ?? _defaultConfig);
        //    await PayPalClient.DetachPaymentMethodToCustomerAsync(paymentMethodID, config);
        //}
        #endregion

        public PayPalService(OCIntegrationConfig defaultConfig) : base(defaultConfig)
        {
        }

        public Task<List<PCISafeCardDetails>> ListSavedCardsAsync(string customerID, OCIntegrationConfig configOverride = null)
        {
            throw new NotImplementedException();
        }

        public Task<PCISafeCardDetails> GetSavedCardAsync(string customerID, string cardID, OCIntegrationConfig configOverride = null)
        {
            throw new NotImplementedException();
        }

        public Task<CardCreatedResponse> CreateSavedCardAsync(PaymentSystemCustomer customer, PCISafeCardDetails card,
            OCIntegrationConfig configOverride = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSavedCardAsync(string customerID, string cardID, OCIntegrationConfig configOverride = null)
        {
            throw new NotImplementedException();
        }
    }
}
