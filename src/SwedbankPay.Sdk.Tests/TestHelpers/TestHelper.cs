﻿using Microsoft.Extensions.Configuration;

namespace SwedbankPay.Sdk.Tests.TestHelpers
{
    public class TestHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets("55739ea0-5447-45e4-b35e-e0412f172f5f")
                .Build();
        }


        public static SwedbankPayConnectionSettings GetSwedbankPayConnectionSettings(string outputPath)
        {
            var configuration = new SwedbankPayConnectionSettings();

            var iConfig = GetIConfigurationRoot(outputPath);

            iConfig
                .GetSection("SwedbankPayConnectionSettings")
                .Bind(configuration);

            return configuration;
        }


        public static Urls GetUrls(string outputPath)
        {
            var configuration = new UrlsOptions();

            var iConfig = GetIConfigurationRoot(outputPath);

            iConfig
                .GetSection("Urls")
                .Bind(configuration);

            return new Urls(configuration.HostUrls, configuration.CompleteUrl, configuration.TermsOfServiceUrl, configuration.CancelUrl,
                            configuration.PaymentUrl, configuration.CallbackUrl, configuration.LogoUrl);
        }
    }
}