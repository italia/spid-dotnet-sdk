using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SpidNetSdk;
using System.IO;
using SpidNetSdk.Saml2;

namespace AspNetSpidSample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AppSettings app = new AppSettings()
            {
                AssertionConsumerServiceUrl = "http://localhost:60981/SamlConsumer/Consume.aspx",
                Issuer = "spid-aspnet"
            };
            SPIDProvider provider = SPIDProvidersFactory.GetProvider("MyIdP", app);
            if (provider.Protocol == SPIDProtocols.SAML2)
            {
                SPIDSaml saml = (SPIDSaml)provider;
                Response.Redirect(provider.GetRedirect());
            }
            else
                throw new NotImplementedException("Only SAML2 supported at the moment");
        }
    }
}