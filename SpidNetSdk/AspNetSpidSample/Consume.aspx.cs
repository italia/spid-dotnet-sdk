using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

using SpidNetSdk;
using System.IO;
using SpidNetSdk.Saml2;

public partial class Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // replace with an instance of the users account.
        SPIDProvider provider = SPIDProvidersFactory.GetProvider("MyIdP", null);

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
