﻿namespace SwedbankPay.Sdk
{
    using System;
    using System.Collections.Generic;

    public class SwedbankPayOptions
    {
        public Uri ApiBaseUrl { get; set; }
        public string Token { get; set; }
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }

        public Uri CallBackUrl { get; set; }
        public Uri CancelPageUrl { get; set; }
        public Uri CompletePageUrl { get; set; }
        public Uri PaymentUrl { get; set; }
        public Uri TermsOfServiceUrl { get; set; }
        public List<Uri> HostUrls { get; set; }
        public Uri LogoUrl { get; set; }

        public bool IsEmpty()
        {
            return ApiBaseUrl == null || string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(MerchantId);
        }
    }
}