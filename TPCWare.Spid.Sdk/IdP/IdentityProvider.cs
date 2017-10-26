using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk.IdP
{
    public class IdentityProvider
    {

        public string ProviderName { get; private set; }

        public string SpidServiceUrl { get; private set; }
        public string SingleLogoutServiceUrl { get; private set; }

        public Func<DateTime, string> Now;
        public Func<DateTime, string> After;
        public Func<DateTime, string> NotBefore;

        public IdentityProvider(string providerName, string spidServiceUrl, string singleLogoutServiceUrl, Func<DateTime, string> now, Func<DateTime, string> after, Func<DateTime, string> notBefore)
        {
            ProviderName = providerName;
            SpidServiceUrl = spidServiceUrl;
            Now = now;
            After = after;
            NotBefore = notBefore;
        }
    }
}
