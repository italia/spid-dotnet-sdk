using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace Developers.Italia.SPID.Test.AspNetMvc5.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: Home/Spid/5
        public ActionResult Spid(int id)
        {

            string destinationUrl = "https://spidposte.test.poste.it/jod-fs/ssoservicepost";
            string serviceProviderId = "https://www.dotnetcode.it";
            string returnUrl = "https://localhost:44355/spidsaml/acs";

            SAML.AuthRequestOptions requestOptions = new SAML.AuthRequestOptions()
            {
                AssertionConsumerServiceIndex = 0,
                AttributeConsumingServiceIndex = 2,
                Destination = destinationUrl,
                SPIDLevel = SAML.SPIDLevel.SPIDL1,
                SPUID = serviceProviderId,
                UUID = Guid.NewGuid().ToString()
            };

            SAML.AuthRequest request = new SAML.AuthRequest(requestOptions);

            X509Certificate2 signinCert = new X509Certificate2("C:\\SourceCode\\spid-dotnet-sdk\\test\\Developers.Italia.SPID.Test\\Certificates\\Hackathon\\www_dotnetcode_it.pfx", "P@ssw0rd!", X509KeyStorageFlags.Exportable);


            string saml = request.GetSignedAuthRequest(signinCert);


            ViewData["saml"] = saml;
            ViewData["FormUrlAction"] = destinationUrl;
            ViewData["SAMLRequest"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(saml));
            ViewData["RelayState"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl));

            return View();
        }

    }
}