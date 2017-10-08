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
                    Certificate =   "-----BEGIN CERTIFICATE-----" +
                                    "MIICNTCCAZ6gAwIBAgIES343gjANBgkqhkiG9w0BAQUFADBVMQswCQYDVQQGEwJVUzELMAkGA1UE\n" +
                                    "CAwCQ0ExFjAUBgNVBAcMDU1vdW50YWluIFZpZXcxDTALBgNVBAoMBFdTTzIxEjAQBgNVBAMMCWxv\n" +
                                    "Y2FsaG9zdDAeFw0xMDAyMTkwNzAyMjZaFw0zNTAyMTMwNzAyMjZaMFUxCzAJBgNVBAYTAlVTMQsw\n" +
                                    "CQYDVQQIDAJDQTEWMBQGA1UEBwwNTW91bnRhaW4gVmlldzENMAsGA1UECgwEV1NPMjESMBAGA1UE\n" +
                                    "AwwJbG9jYWxob3N0MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCUp/oV1vWc8/TkQSiAvTou\n" +
                                    "sMzOM4asB2iltr2QKozni5aVFu818MpOLZIr8LMnTzWllJvvaA5RAAdpbECb+48FjbBe0hseUdN5\n" +
                                    "HpwvnH/DW8ZccGvk53I6Orq7hLCv1ZHtuOCokghz/ATrhyPq+QktMfXnRS4HrKGJTzxaCcU7OQID\n" +
                                    "AQABoxIwEDAOBgNVHQ8BAf8EBAMCBPAwDQYJKoZIhvcNAQEFBQADgYEAW5wPR7cr1LAdq+IrR44i\n" +
                                    "QlRG5ITCZXY9hI0PygLP2rHANh+PYfTmxbuOnykNGyhM6FjFLbW2uZHQTY1jMrPprjOrmyK5sjJR\n" +
                                    "O4d1DeGHT/YnIjs9JogRKv4XHECwLtIVdAbIdWHEtVZJyMSktcyysFcvuhPQK8Qc/E/Wq8uHSCo=\n" +
                                    "-----END CERTIFICATE-----\n",
                    IdpSsoBaseUrl = "https://idp.spid.gov.it:9443",
                    IdpSsoTargetUrl = "/samlsso"
                }
                ) },
            { "OidcDemo", (
                SPIDProtocols.OIDC,
                new OidcAccountSettings()
                {
                    OidcAuthority = "https://demo.identityserver.io"
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

        public static SPIDOidc GetOidcProvider(string idp, AppSettings app)
        {
            return new SPIDOidc((OidcAccountSettings)providerSpecs[idp].Item2, app);
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
                        return GetOidcProvider(idp, app);
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

