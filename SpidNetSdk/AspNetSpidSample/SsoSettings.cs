using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SpidNetSdk;
using SpidNetSdk.OidConnect.Browsers;

namespace AspNetSpidSample
{
    public class SsoSettings
    {
        public static AppSettings WebFormsApp { get { return app; } }

        private static readonly AppSettings app = new AppSettings()
        {
            Issuer = "spid-aspnet",

            SamlAssertionConsumerServiceUrl = "http://localhost:60981/SamlConsumer.aspx",
            SpKeysPath = HttpContext.Current.Server.MapPath("~/spKeys.pfx"),
            SpKeysPassword = "test",

            OidcCallbackUri = "http://localhost:60981/OidcConsumer.aspx",
            OidcScope = "openid profile api offline_access",
            OidcBrowser = new RedirectBrowser()
        };
    }
}