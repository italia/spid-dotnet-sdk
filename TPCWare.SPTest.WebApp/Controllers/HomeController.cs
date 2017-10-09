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
using TPCWare.SPTest.SAML;
using TPCWare.SPTest.WebApp.Models;

namespace TPCWare.SPTest.WebApp.Controllers
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

        public ActionResult SpidRequest(string idP)
        {
              
            ThreadContext.Properties["Provider"] = idP;

            try
            {
                string spidServiceUrl;

                switch (idP)
                {
                    case "poste_id":
                        spidServiceUrl = "https://spidposte.test.poste.it/jod-fs/ssoservicepost";
                        break;
                    case "tim_id":
                        spidServiceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");


                    case "sielte_id":
                        spidServiceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                    case "infocert_id":
                        spidServiceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                    default:
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                }

                int securityLevelSPID = 1;

                Guid spidIdRequest = Guid.NewGuid();

                HttpCookie requestCookie = new HttpCookie(ConfigurationManager.AppSettings["SPID_COOKIE"].ToString());

                requestCookie.Expires = DateTime.Now.AddMinutes(30);

                requestCookie.Value = spidIdRequest.ToString();

                System.Web.HttpContext.Current.Response.Cookies.Add(requestCookie);

                var spidCryptoRequest = Saml2Helper.BuildPostSamlRequest("_" + spidIdRequest.ToString(), 
                       spidServiceUrl,
                       ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"], securityLevelSPID,
                       null,
                       null, 
                       StoreLocation.LocalMachine,
                       StoreName.My,
                       X509FindType.FindBySubjectName,
                       ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"], 
                       idP,
                       ConfigurationManager.AppSettings["ENVIROMENT"].ToString() == "dev" ? 1 : 0);

                byte[] base64EncodedBytes = Encoding.UTF8.GetBytes(spidCryptoRequest);

                string returnValue = System.Convert.ToBase64String(base64EncodedBytes);

                ViewData["data"] = returnValue;

                ViewData["action"] = spidServiceUrl;

                return View("PostData");

            }
            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore", ex);
               
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