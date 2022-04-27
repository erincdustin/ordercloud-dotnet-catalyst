﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCloud.Integrations.Payment.BlueSnap
{
	public class BlueSnapVaultedShopper
	{
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string softDescriptor { get; set; }
		public string descriptorPhoneNumber { get; set; }
		public string merchantShopperId { get; set; }
		public string country { get; set; }
		public string state { get; set; }
		public string city { get; set; }
		public string address { get; set; }
		public string address2 { get; set; }
		public string email { get; set; }
		public string zip { get; set; }
		public string phone { get; set; }
		public string companyName { get; set; }
		public string shopperCurrency { get; set; }
		public long walletId { get; set; }
		public string transactionOrderSource { get; set; }
		public BlueSnapShippingContactInfo shippingContactInfo { get; set; }
		public BlueSnapTransactionFraudInfo transactionFraudInfo { get; set; }
		public BlueSnapPaymentSources paymentSources { get; set; }
		public BlueSnap3DSecure threeDSecure { get; set; }
	}

	/// <summary>
	/// https://developers.bluesnap.com/v8976-JSON/docs/payment-sources
	/// </summary>
	public class BlueSnapPaymentSources
	{
		public List<BlueSnapCreditCardInfo> creditCardInfo { get; set; }
	}

	public class BlueSnapCreditCardInfo
	{
		public BlueSnapBillingContactInfo billingContactInfo { get; set; }
		public BlueSnapCreditCard creditCard { get; set; }
		public string pfToken { get; set; }
		public string status { get; set; }
	}

	/// <summary>
	/// https://developers.bluesnap.com/v8976-JSON/docs/threedsecure
	/// </summary>
	public class BlueSnap3DSecure
	{
		public string acsTransactionId { get; set; }
		public string cavv { get; set; }
		public string cavvAlgorithm { get; set; }
		public bool challengeRequired { get; set; }
		public string dsTransactionId { get; set; }
		public string eci { get; set; }
		public string enrollmentStatus { get; set; }
		public string networkScore { get; set; }
		public string reasonCode { get; set; }
		public string threeDSecureReferenceId { get; set; }
		public string threeDSecureStatus { get; set; }
		public string threeDSecureVersion { get; set; }
		public string xid { get; set; }
	}
}
