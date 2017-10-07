using SpidNetSdk.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk
{
    public enum SPIDProviders
    {
        MyIdP,
        PosteID
    }

    public static class SPIDProvidersFactory
    {
        private static Dictionary<SPIDProviders, (AccountSettings, AppSettings)> samlSpecs = new Dictionary<SPIDProviders, (AccountSettings, AppSettings)>()
        {
            { SPIDProviders.MyIdP, (
                new AccountSettings()
                {
                    Certificate = "blablabla",
                    IdpSsoTargetUrl = "https://app.onelogin.com/saml/signon/20219"
                },
                new AppSettings()
                {
                    AssertionConsumerServiceUrl = "http://localhost:49573/SamlConsumer/Consume.aspx",
                    Issuer = "test-app"
                }
                ) },
            { SPIDProviders.PosteID, (null, null) }
        };

        public static SPIDProvider GetOicdProvider(SPIDProviders idp)
        {
            throw new NotImplementedException();
        }

        public static SPIDSaml GetSamlProvider(SPIDProviders idp)
        {
            return new SPIDSaml(samlSpecs[idp].Item1, samlSpecs[idp].Item2);
        }
    }
}
