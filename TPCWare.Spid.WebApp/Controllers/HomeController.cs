using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using TPCWare.Spid.Sdk;
using TPCWare.Spid.Sdk.IdP;
using TPCWare.Spid.WebApp.Models;

namespace TPCWare.Spid.WebApp.Controllers
{
   
    public class HomeController : Controller
    {

        ILog Log = log4net.LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            if (Session["AppUser"] != null)
            {
                ViewBag.Name = ((AppUser)Session["AppUser"]).Name;

                ViewBag.Surname = ((AppUser)Session["AppUser"]).Surname;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Il progetto è stato realizzato da Nicolò Carandini e Antimo Musone per l'evento Hack Developers del 07-10-2017.";

            return View();
        }

        public ActionResult SpidRequest(string idpLabel)
        {
            ThreadContext.Properties["Provider"] = idpLabel;

            try
            {
                // Select the Identity Provider
                IdentityProvider idp = IdentityProviderSelector.GetIdpFromUserChoice(idpLabel, forTesting: true);

                // Create the SPID request id and save it as a cookie
                string spidIdRequest = Guid.NewGuid().ToString();
                System.Web.HttpContext.Current.Response.Cookies.Add(new HttpCookie(ConfigurationManager.AppSettings["SPID_COOKIE"])
                {
                    Expires = DateTime.Now.AddMinutes(30),
                    Value = spidIdRequest
                });

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"],
                    validOnly: false);

                // Create the signed SAML request
                var spidCryptoRequest = Saml2Helper.BuildPostSamlRequest(
                    uuid: spidIdRequest,
                    destination: idp.SpidServiceUrl,
                    consumerServiceURL: ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"],
                    securityLevel: 1,
                    certificate: certificate,
                    identityProvider: idp,
                    enviroment: ConfigurationManager.AppSettings["ENVIROMENT"] == "dev" ? 1 : 0);

                ViewData["data"] = spidCryptoRequest;
                ViewData["action"] = idp.SpidServiceUrl;

                return View("PostData");
            }
            catch (Exception ex)
            {
                Log.Error("Error on HomeController SpidRequest", ex);
                ViewData["Message"] = "Errore nella preparazione della richiesta SPID da inviare al provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Nicolò Carandini n.carandini@outlook.com , Antimo Musone antimo.musone@hotmail.com ";
            return View();
        }

    }
}