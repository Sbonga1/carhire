using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CarRentalSystem
{
    public static class PayPalConfig
    {
        public static string ClientId { get; set; } = ConfigurationManager.AppSettings["PayPalClientId"];
        public static string ClientSecret { get; set; } = ConfigurationManager.AppSettings["PayPalClientSecret"];

        public static APIContext GetAPIContext()
        {
            var config = new Dictionary<string, string> { { "mode", "sandbox" } };
            var accessToken = new OAuthTokenCredential(ClientId, ClientSecret, config).GetAccessToken();
            var apiContext = new APIContext(accessToken);
            apiContext.Config = config;
            return apiContext;
        }
    }
}