using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SpidNetSdk;
using System.IO;

namespace AspNetSpidSample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Command(object sender, CommandEventArgs e)
        {
            Response.Redirect(SPIDProvidersFactory.GetSamlProvider(SPIDProviders.MyIdP).GetSamlRedirect());
        }
    }
}