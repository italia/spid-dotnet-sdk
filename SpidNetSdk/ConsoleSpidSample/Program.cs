using SpidNetSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSpidSample
{
    class Program
    {
        static void Main(string[] args)
        {
            AppSettings app = new AppSettings() {
                SamlAssertionConsumerServiceUrl = "redirectTarget",
                Issuer = "spid-console"
            };

            var p = SPIDProvidersFactory.GetProvider("DemoIdP", app);

            p.GetRedirect();

            // navigate that url, follow redirects, show IdP web form, intercept consumer redirection and consume it
        }
        
    }
}
