using System;
using Microsoft.AspNetCore.Mvc;
using TPCWare.SPTest.AspNetCore.WebApp.Models;
using Microsoft.AspNetCore.Http;
using log4net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Options;
using TPCWare.SPTest.SAML2;

namespace TPCWare.SPTest.AspNetCore.WebApp.Controllers
{
    public class HomeController : Controller
    {
        ILog Log = log4net.LogManager.GetLogger(typeof(HomeController));

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SpidOptions _spidOptions;

        public HomeController(IOptions<SpidOptions> spidOptionsAccessor, IHttpContextAccessor contextAccessor)
        {
            _spidOptions = spidOptionsAccessor.Value;
            _contextAccessor = contextAccessor;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Il progetto è stato realizzato da Nicolò Carandini e Antimo Musone per l'evento Hack Developers del 07-10-2017.";

            return View();
        }

        public ActionResult SpidRequest(string idP)
        {

            HttpContext CurrentContext = _contextAccessor.HttpContext;

            ThreadContext.Properties["Provider"] = idP;

            try
            {
                string serviceUrl = "";

                switch (idP)
                {
                    case "poste_id":
                        serviceUrl = "https://spidposte.test.poste.it/jod-fs/ssoservicepost";
                        break;
                    case "tim_id":
                        serviceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");


                    case "sielte_id":
                        serviceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                    case "infocert_id":
                        serviceUrl = "#";
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                    default:
                        ViewData["Message"] = "Ci dispiace ma il sistema di test non è supportato.";
                        return View("Error");

                }

                int securityLevelSPID = 1;

                Guid spidIdRequest = Guid.NewGuid();
                 

                var spidCryptoRequest = Saml2Helper.BuildPostSamlRequest("_" + spidIdRequest.ToString(),
                    serviceUrl,
                    _spidOptions.DomainValue,
                    securityLevelSPID,
                       null,
                       null,
                       StoreLocation.LocalMachine,
                       StoreName.My,
                       X509FindType.FindBySubjectName,
                       _spidOptions.CertificateName,
                       idP,
                       1);

                byte[] base64EncodedBytes = Encoding.UTF8.GetBytes(spidCryptoRequest);

                string returnValue = System.Convert.ToBase64String(base64EncodedBytes);

                ViewData["data"] = returnValue;
                ViewData["action"] = serviceUrl;

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