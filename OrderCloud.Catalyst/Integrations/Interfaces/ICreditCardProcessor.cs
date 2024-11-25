﻿using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderCloud.Catalyst
{
	/// <summary>
	/// An interface to define the behavior of a credit card processor. Full CC details are never passed to it, as that would put it in PCI compliance scope. Instead, it accepts iframe generated tokens or saved card IDs.
	/// </summary>
	public interface ICreditCardProcessor
	{
		/// <summary>
		/// Get a string credential needed for the client-side Iframe. This may mean slightly different things for different processors, so consult the documentation.
		/// </summary>
		Task<string> GetIFrameCredentialAsync(OCIntegrationConfig overrideConfig = null);
		/// <summary>
		/// Create the payment request to initialize payment processing. Optionally define whether the intent is authorization only, or capture.
		/// </summary>
        Task<CCTransactionResult> InitializePaymentRequestAsync(AuthorizeCCTransaction transaction, OCIntegrationConfig overrideConfig = null, bool isCapture = false);
		/// <summary>
		/// Attempt to verify the user can pay by placing a hold on a credit card. Funds will be captured later. Typically used as a verification step directly before order submit.
		/// </summary>
		Task<CCTransactionResult> AuthorizeOnlyAsync(AuthorizeCCTransaction transaction, OCIntegrationConfig overrideConfig = null);
		/// <summary>
		/// Attempt to capture funds from a credit card. A prior authorization is required. Typically used when a shipment is created, at the end of the day, or a defined time period after submit.
		/// </summary>
		Task<CCTransactionResult> CapturePriorAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null);
        /// <summary>
        /// Capture funds at the time of the transaction without prior authorization.
        /// </summary>
        Task<CCTransactionResult> CapturePaymentAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null);
        /// <summary>
        /// Remove an authorization hold previously placed on a credit card. Use if order submit fails, or if order is canceled/returned before capture. 
        /// </summary>
        Task<CCTransactionResult> VoidAuthorizationAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null);
		/// <summary>
		/// Refund a previously captured amount. Used if an order is canceled/returned after capture. Refunding generally incures extra processing fees, whereas voiding does not.
		/// </summary>
		Task<CCTransactionResult> RefundCaptureAsync(FollowUpCCTransaction transaction, OCIntegrationConfig overrideConfig = null);
	}

    public class AuthorizeCCTransaction
	{
		/// <summary>
		/// The OrderCloud Order ID that this card transaction applies to.
		/// </summary>
		public string OrderID { get; set; }
		/// <summary>
		/// The amount that will be authorized on the credit card.
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// The currency to authorize in - three letter ISO format. 
		/// </summary>
		public string Currency { get; set; }
		/// <summary>
		/// Card details. The Token or SavedCardID will be what is used to perform authorization.
		/// </summary>
		public PCISafeCardDetails CardDetails { get; set; }
		/// <summary>
		/// The ID of a customer record in the processor system. Needed if paying with a saved credit card.
		/// </summary>
		public string ProcessorCustomerID { get; set; }
		/// <summary>
		/// Address verification (AVS) is an optional layer of security for payments. It checks a customer-provided street and zip code against the records on file with the card issuer. 
		/// </summary>
		public Address AddressVerification {get; set; }
		/// <summary>
		/// The customer's IP address is typically not required by processors, but it provides a layer of insurance on disputed or fraudulent payments. 
		/// </summary>
		public string CustomerIPAddress { get; set; }
		/// <summary>
		/// Implementations of this interface may choose to ignore this or use it as they choose. Never use XP properties.
		/// </summary>
		public OrderWorksheet OrderWorksheet { get; set; }
        /// <summary>
        /// An optional header value used by some processors to enforce idempotency.
        /// </summary>
        public string RequestID { get; set; }
	}

	public class CCTransactionResult
	{
		/// <summary>
		/// Did the transaction succeed?
		/// </summary>
		public bool Succeeded { get; set; }
		/// <summary>
		/// The amount of the transaction   
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// The processor-generated ID for this action. Null if a create attempt failed. 
		/// </summary>
		public string TransactionID { get; set; }
		/// <summary>
		/// The raw processor-specific response code. Depending on the processor, typical meanings include Approved, Declined, Held For Review, Retry, Error.
		/// </summary>
		public string ResponseCode { get; set; }
		/// <summary>
		/// The authorization code granted by the card issuing bank for this transaction. Should be 6 characters, e.g. "HH5414".
		/// </summary>
		public string AuthorizationCode { get; set; }
		/// <summary>
		/// A code explaining the result of address verification (AVS). Whether to perform AVS is typically configured at the processor level. Standard 1 character result codes, see https://www.merchantmaverick.com/what-is-avs-for-credit-card-processing/.  
		/// </summary>
		public string AVSResponseCode { get; set; }
		/// <summary>
		/// User readable text explaining the result.
		/// </summary>
		public string Message { get; set; }
        /// <summary>
        /// The ID of the merchant associated with the transaction
        /// </summary>
        public string MerchantID { get; set; }
        /// <summary>
        /// If there are multiple merchant captures processed, store each response in a nested CCTransactionResult
        /// </summary>
        public List<CCTransactionResult> InnerTransactions { get; set; }
    }

	/// <summary>
	/// A credit card transaction that follows after a successful authorization such as capture, void, or refund.
	/// </summary>
	public class FollowUpCCTransaction
	{
		/// <summary>
		/// The processor-generated ID of the original authorize transaction.
		/// </summary>
		public string TransactionID { get; set; }
		/// <summary>
		/// The amount to capture, void, or refund. If null, will default to the full amount of the existing transaction.
		/// </summary>
		public decimal Amount { get; set; }
        /// <summary>
        /// An optional header value used by some processors to enforce idempotency.
        /// </summary>
        public string RequestID { get; set; }
    }
}
