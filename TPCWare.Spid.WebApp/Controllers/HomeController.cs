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
using TPCWare.Spid.Sdk.Schema;
using TPCWare.Spid.WebApp.Models;

namespace TPCWare.Spid.WebApp.Controllers
{

    public class HomeController : Controller
    {
        private ILog log = LogManager.GetLogger(typeof(HomeController));
        private readonly string SPID_COOKIE = ConfigurationManager.AppSettings["SPID_COOKIE"];

        public ActionResult Index()
        {
            if (Session["AppUser"] != null)
            {
                ViewBag.Name = ((AppUser)Session["AppUser"]).Name;
                ViewBag.Surname = ((AppUser)Session["AppUser"]).Surname;
                ViewBag.Logged = true;
            }
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Nicolò Carandini n.carandini@outlook.com , Antimo Musone antimo.musone@hotmail.com ";
            return View();
        }

        public ActionResult SpidRequest(string idpLabel)
        {
            try
            {
                // Create the SPID request id
                string spidRequestId = Guid.NewGuid().ToString();

                // Select the Identity Provider
                IdentityProvider idp = IdentityProviderSelector.GetIdpFromIdPLabel(idpLabel, forTesting: true);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"],
                    validOnly: false);

                // Create the signed SAML request
                var spidAuthnRequest = Saml2Helper.BuildSpidAuthnPostRequest(
                    uuid: spidRequestId,
                    destination: idp.SpidServiceUrl,
                    consumerServiceURL: ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"],
                    securityLevel: 1,
                    certificate: certificate,
                    identityProvider: idp,
                    enviroment: ConfigurationManager.AppSettings["ENVIROMENT"] == "dev" ? 1 : 0);

                ViewData["data"] = spidAuthnRequest;
                ViewData["action"] = idp.SpidServiceUrl;

                // Save the IdP label and SPID request id as a cookie
                HttpCookie cookie = Request.Cookies.Get(SPID_COOKIE) ?? new HttpCookie(SPID_COOKIE);
                cookie.Values["IdPLabel"] = idpLabel;
                cookie.Values["SpidRequestId"] = spidRequestId;
                cookie.Expires = DateTime.Now.AddMinutes(20);
                Response.Cookies.Add(cookie);

                // Send the request to the Identity Provider
                return View("PostData");
            }
            catch (Exception ex)
            {
                log.Error("Error on HomeController SpidRequest", ex);
                ViewData["Message"] = "Errore nella preparazione della richiesta di autenticazione da inviare al provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public ActionResult LogoutRequest()
        {
            string idpLabel;
            string spidRequestId;
            string subjectNameId;
            string authnStatementSessionIndex;

            // Try to get Authentication data from cookie
            HttpCookie cookie = Request.Cookies[SPID_COOKIE];

            if (cookie == null)
            {
                log.Error("Error on HomeController LogoutRequest method: Impossibile recuperare i dati della sessione (cookie scaduto)");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (cookie scaduto).";
                return View("Error");
            }

            idpLabel = cookie["IdPLabel"];
            spidRequestId = cookie["SpidRequestId"];
            subjectNameId = cookie["SubjectNameId"];
            authnStatementSessionIndex = cookie["AuthnStatementSessionIndex"];

            if (string.IsNullOrWhiteSpace(idpLabel) ||
                string.IsNullOrWhiteSpace(spidRequestId) ||
                string.IsNullOrWhiteSpace(subjectNameId) ||
                string.IsNullOrWhiteSpace(authnStatementSessionIndex))
            {
                log.Error("Error on HomeController LogoutRequest method: Impossibile recuperare i dati della sessione (il cookie non contiene tutti i dati necessari)");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (il cookie non contiene tutti i dati necessari).";
                return View("Error");
            }

            try
            {
                // Create the SPID request id and save it as a cookie
                string logoutRequestId = Guid.NewGuid().ToString();

                // Select the Identity Provider
                IdentityProvider idp = IdentityProviderSelector.GetIdpFromIdPLabel(idpLabel, forTesting: true);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"],
                    validOnly: false);

                // Create the signed SAML logout request
                var spidLogoutRequest = Saml2Helper.BuildSpidLogoutPostRequest(
                    uuid: logoutRequestId,
                    destination: idp.LogoutServiceUrl,
                    consumerServiceURL: ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"],
                    certificate: certificate,
                    identityProvider: idp,
                    subjectNameId: subjectNameId.Replace("SPID-",""),
                    authnStatementSessionIndex: authnStatementSessionIndex);

                ViewData["data"] = spidLogoutRequest;
                ViewData["action"] = idp.LogoutServiceUrl;

                // Add the NameID and save the authorization data as a cookie
                cookie = new HttpCookie("userInfo");
                cookie.Values["IdPLabel"] = idpLabel;
                cookie.Values["SpidRequestId"] = spidRequestId;
                cookie.Expires = DateTime.Now.AddMinutes(20);
                Response.Cookies.Add(cookie);

                // Send the request to the Identity Provider
                return View("PostData");
            }
            catch (Exception ex)
            {
                log.Error("Error on HomeController SpidRequest", ex);
                ViewData["Message"] = "Errore nella preparazione della richiesta di logout da inviare al provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public JsonResult CheckSpidLogin(string cf)
        {


            try
            {

                List<AppUser> logged = (List<AppUser>)System.Web.HttpContext.Current.Application["Users"];

                var item = logged.Where(x => x.FiscalNumber.ToUpper() == cf.ToUpper()).FirstOrDefault();

                if (item != null)
                    return Json(new { result = "true", data = item }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { result = "false" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return null;
            }
        }

    }
}