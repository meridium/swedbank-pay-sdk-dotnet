﻿using System.Collections.Generic;

namespace SwedbankPay.Sdk.PaymentOrders
{
    public class PaymentOrderReversalTransaction
    {
        protected internal PaymentOrderReversalTransaction(Amount amount,
                                               Amount vatAmount,
                                               List<OrderItem> orderItems,
                                               string description,
                                               string payeeReference)
        {
            Amount = amount;
            VatAmount = vatAmount;
            OrderItems = orderItems;
            Description = description;
            PayeeReference = payeeReference;
        }


        public Amount Amount { get; }
        public string Description { get; }
        public List<OrderItem> OrderItems { get; }
        public string PayeeReference { get; }
        public Amount VatAmount { get; }
    }
}