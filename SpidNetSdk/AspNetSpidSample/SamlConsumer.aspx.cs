using SpidNetSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNetSpidSample
{

    public partial class SamlConsumer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // replace with an instance of the users account.
            SPIDProvider provider = SPIDProvidersFactory.GetProvider("DemoIdP", null);

            string result = provider.Consume(Request.Form["SAMLResponse"]);

            if (result != "")
            {
                Response.Write("OK!");
                Response.Write(result);
            }
            else
            {
                Response.Write("Failed");
            }
        }
    }
}