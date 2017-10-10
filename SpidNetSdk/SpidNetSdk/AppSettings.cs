using IdentityModel.OidcClient.Browser;
using SpidNetSdk.OidConnect.Browsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk
{
    public class AppSettings
    {
        public string Issuer { get; set; }

        public string SamlAssertionConsumerServiceUrl { get; set; }
        public string SpKeysPath { get; set; }
        public string SpKeysPassword { get; set; }

        public string OidcScope { get; set; }
        public string OidcCallbackUri { get; set; }
        public IBrowser OidcBrowser { get; set; }
    }
}
