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
            ViewBag.Message = "Hai invocato idp poste";

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

                Guid IdRichiesta = Guid.NewGuid();

                //HttpCookie requestCookie = new HttpCookie(ConfigurationManager.AppSettings["SPID_COOKIE"].ToString());
                //requestCookie.Expires = DateTime.Now.AddMinutes(20);
                //requestCookie.Value = IdRichiesta.ToString();
                //CurrentContext.Response.Cookies.Add(requestCookie);

                string cryptrequest = SamlHelper.GetPostSamlRequest("_" + IdRichiesta.ToString(), serviceUrl, _spidOptions.DomainValue, securityLevelSPID,
                       null, null, StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName,
                       _spidOptions.CertificateName, idP, 1);

                byte[] base64EncodedBytes = Encoding.UTF8.GetBytes(cryptrequest);
                string returnValue = System.Convert.ToBase64String(base64EncodedBytes);

                ViewData["data"] = returnValue;
                ViewData["action"] = serviceUrl;

                return View("PostData");

            }
            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore", ex);
                ViewData["Message"] = "Ci dispiace ma si è verificato un Errore, si prega di riprovare";
                return View("Error");
            }

            
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Nicolò Carandini n.carandini@outlook.com , Antimo Musone antimo.musone@hotmail.com ";

            return View();
        }

        

        //[HttpPost]
        //public ActionResult AutoResponse(RichiestaManuale model, FormCollection collection)
        //{

        //    HttpContext CurrentContext = System.Web.HttpContext.Current;
        //    ThreadContext.Properties["Provider"] = 0;
        //    ThreadContext.Properties["Application"] = 0;
        //    string SPID_Domain = ConfigurationManager.AppSettings[SPID_DOMAIN_VALUE];
        //    Guid idRequest;
        //    SPID.Model.Richiesta request;
        //    SPID.Model.Applicazione app;
        //    string codFiscaleIva = String.Empty;

        //    try
        //    {
        //        using (SPID_DBEntities db = new SPID_DBEntities())
        //        {
        //            string spidcookie = ConfigurationManager.AppSettings[SPID_COOKIE];
        //            SPID.Model.Settings set = db.SP_SETTINGS_Get().FirstOrDefault();

        //            if (set != null && set.CourtesyPage)
        //            {
        //                ViewData["Message"] = "Ci dispiace ma il servizio momentaneamente non è disponibile";
        //                Log.Error("SPID Disabilitato");
        //                return View("Error");
        //            }

        //            string backUrl = String.Empty;
        //            string requestQueryString = String.Empty;
        //            string appQueryString = String.Empty;
        //            string finalbackUrl = String.Empty;

        //            if (CurrentContext.Request.Cookies[spidcookie] != null)
        //            {
        //                idRequest = Guid.Parse(CurrentContext.Request.Cookies[spidcookie].Value.ToString());

        //                request = db.SP_REQUEST_Get(idRequest).FirstOrDefault();
        //                app = db.SP_APPLICATION_Get(request.IdApplicazione).FirstOrDefault();
        //                ThreadContext.Properties["Application"] = app.IdApplicazione;

        //                if (!app.Abilitata)
        //                {
        //                    Log.Error("L'Applicazione non è Abilitata:: Application " + app.IdApplicazione + " - IdentityProvider: " + request.IdIdentityProvider);
        //                    ViewData["Message"] = "Ci dispiace ma si è verificato un errore.";
        //                    return View("Error");
        //                }

        //                if (request.DataRichiesta.AddMinutes(request.Applicazione.DurataCookie) < DateTime.Now)
        //                {
        //                    Log.Warn("Richiesta Scaduta:: Request" + request.IdRichiesta + " - IdentityProvider: " + request.IdIdentityProvider);
        //                    ViewData["Message"] = "La Richiesta è scaduta, si prega di riprovare";
        //                    return View("Error");
        //                }

        //                Log.Info("Recupero Richiesta: " + idRequest + " - IdentityProvider: " + request.IdIdentityProvider);
        //                requestQueryString = request.ParametriQuery;
        //                backUrl = app.UrlDiSuccesso;
        //                appQueryString = app.ParametriQuery;
        //                if (!String.IsNullOrEmpty(requestQueryString))
        //                {
        //                    if (!String.IsNullOrEmpty(appQueryString))
        //                    {
        //                        finalbackUrl = backUrl + "?" + requestQueryString + "&" + appQueryString;
        //                    }
        //                    else
        //                    {
        //                        finalbackUrl = backUrl + "?" + requestQueryString;
        //                    }
        //                }
        //                else if (!String.IsNullOrEmpty(appQueryString))
        //                {
        //                    finalbackUrl = backUrl + "?" + appQueryString;
        //                }
        //                else
        //                {
        //                    finalbackUrl = backUrl;
        //                }
        //            }
        //            else
        //            {
        //                Log.Warn("Cookie non trovato, impossibile proseguire.");
        //                ViewData["Message"] = "Ci dispiace ma si è verificato un errore, assicurati di avere abilitato i Cookie";
        //                return View("Error");
        //            }

        //            Dictionary<string, string> userInfo = new Dictionary<string, string>();

        //            Utility.attributeSetting(out userInfo, out codFiscaleIva, model, Log);

        //            string cryptresponse = SamlHelper.GetPostSamlResponse(
        //            backUrl,
        //            SPID_Domain,
        //            SPID_Domain,
        //            "SPID",
        //            StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName,
        //            null, null, ConfigurationManager.AppSettings[SPID_CERTIFICATE_NAME], userInfo);

        //            db.SP_REQUEST_Finalize(idRequest, DateTime.Now, true, codFiscaleIva, cryptresponse).FirstOrDefault();

        //            ViewData["data"] = cryptresponse;
        //            ViewData["action"] = finalbackUrl;
        //            return View("PostData");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Si è verificato un Errore durante il Processamento della Response", ex);
        //        ViewData["Message"] = "Ci dispiace ma si è verificato un Errore, si prega di riprovare";
        //        return View("Error");
        //    }
        //}

    }
}