using SpidNetSdk.OidConnect;
using SpidNetSdk.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk
{
    public static class SPIDProvidersFactory
    {
        private static Dictionary<string, (SPIDProtocols, AccountSettings)> providerSpecs = new Dictionary<string, (SPIDProtocols, AccountSettings)>()
        {
            { "MyIdP", (
                SPIDProtocols.SAML2,
                new SamlAccountSettings()
                {
                    Certificate = "blablabla",
                    IdpSsoTargetUrl = "https://app.onelogin.com/saml/signon/20219"
                }
                ) },
            { "PosteID", (
                SPIDProtocols.SAML2,
                null
                ) },
            { "RegisterIt", (
                SPIDProtocols.OIDC,
                null
                ) }
        };

        public static bool UpdateProviders()
        {
            // fill providerSpecs dynamically
            throw new NotImplementedException();
        }

        public static SPIDOicd GetOicdProvider(string idp, AppSettings app)
        {
            throw new NotImplementedException();
        }

        public static SPIDSaml GetSamlProvider(string idp, AppSettings app)
        {
            return new SPIDSaml((SamlAccountSettings)providerSpecs[idp].Item2, app);
        }

        public static SPIDProvider GetProvider(string idp, AppSettings app)
        {
            if (providerSpecs.ContainsKey(idp))
            {
                var spec = providerSpecs[idp];
                switch (spec.Item1)
                {
                    case SPIDProtocols.OIDC:
                        return GetOicdProvider(idp, app);
                    case SPIDProtocols.SAML2:
                        return GetSamlProvider(idp, app);
                    default:
                        return null;
                }
            }
            else
                throw new Exception("Unsupported IdP");
        }
    }
}

