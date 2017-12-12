using System;
using System.Collections.Generic;
using System.Text;

namespace Developers.Italia.SPID.SAML
{
    public class SpidProviderConfiguration
    {
        public string IdentityProviderId { get; set; }

        public string IdentityProviderName { get; set; }

        public string IdentityProviderLoginPostUrl { get; set; }

        public string IdentityProviderLogoutPostUrl { get; set; }

        public string IdentityProviderCertPath { get; set; }


        public string ServiceProviderId { get; set; }

        public string ServiceProviderCertPath { get; set; }

        public string ServiceProviderCertPassword { get; set; }

        public string ServiceProviderPrivatekey { get; set; }

    }
}

