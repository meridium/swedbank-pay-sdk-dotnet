﻿using SwedbankPay.Sdk.Payments.CardPayments;
using SwedbankPay.Sdk.Payments.SwishPayments;
using SwedbankPay.Sdk.Payments.InvoicePayments;
using SwedbankPay.Sdk.Payments.VippsPayments;
using SwedbankPay.Sdk.Payments.MobilePayPayments;

namespace SwedbankPay.Sdk.Payments
{
    public interface IPaymentsResource
    {
        ICardPaymentsResource CardPayments { get; }
        ISwishPaymentsResource SwishPayments { get; }
        IInvoicePaymentsResource InvoicePayments { get; }
        IVippsPaymentsResource VippsPayments { get; }
        IMobilePayPaymentsResource MobilePayPayments { get; }

    }
}