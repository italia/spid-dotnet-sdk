using SpidNetSdk;
using SpidNetSdk.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNetSpidSample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void LoginButton_Command(object sender, EventArgs e)
        {
            SPIDProvider provider = SPIDProvidersFactory.GetProvider(
                "MyIdP",
                // "OidcDemo",
                SsoSettings.WebFormsApp);

            Response.Redirect(provider.GetRedirect());
        }
    }
}