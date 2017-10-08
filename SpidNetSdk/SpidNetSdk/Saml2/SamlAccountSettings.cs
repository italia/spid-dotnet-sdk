using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk.Saml2
{

    public class SamlAccountSettings : AccountSettings
    {
        public string Certificate { get; set; }
        public string IdpSsoTargetUrl { get; set; }
        public string IdpSsoBaseUrl { get; internal set; }
    }
}
