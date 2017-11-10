using System;

namespace TPCWare.Spid.Sdk.IdP
{
    public class IdentityProvider
    {
        public string ProviderName { get; private set; }

        public string SpidServiceUrl { get; private set; }

        public string LogoutServiceUrl { get; private set; }

        public Func<string, string> SubjectNameIdFormatter;

        public Func<DateTime, string> Now;
        public Func<DateTime, string> After;
        public Func<DateTime, string> NotBefore;

        public IdentityProvider(string providerName,
                                string spidServiceUrl,
                                string logoutServiceUrl,
                                Func<string, string> subjectNameIdFormatter,
                                Func<DateTime, string> now,
                                Func<DateTime, string> after,
                                Func<DateTime, string> notBefore)
        {
            ProviderName = providerName;
            SpidServiceUrl = spidServiceUrl;
            LogoutServiceUrl = logoutServiceUrl;
            SubjectNameIdFormatter = subjectNameIdFormatter;
            Now = now;
            After = after;
            NotBefore = notBefore;
        }
    }
}
