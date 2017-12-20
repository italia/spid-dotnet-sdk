





using Italia.Spid.AspNet.WebApp.Models;
using Italia.Spid.Authentication;
using Italia.Spid.Authentication.IdP;
using Italia.Spid.Authentication.Saml;
using log4net;
using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace Italia.Spid.AspNet.WebApp.Controllers
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
            return View();
        }

        public ActionResult SpidRequest(string idpName)
        {
            try
            {
                // Create the SPID request id
                string spidAuthnRequestId = Guid.NewGuid().ToString();

                // Select the Identity Provider
                IdentityProvider idp = IdentityProvidersList.GetIdpFromIdPName(idpName);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"],
                    validOnly: false);

                // Create the signed SAML request
                var spidAuthnRequest = SamlHelper.BuildAuthnPostRequest(
                    uuid: spidAuthnRequestId,
                    destination: idp.SingleSignOnServiceUrl,
                    consumerServiceURL: ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"],
                    securityLevel: 1,
                    certificate: certificate,
                    identityProvider: idp,
                    enviroment: ConfigurationManager.AppSettings["ENVIROMENT"] == "dev" ? 1 : 0);

                ViewData["data"] = spidAuthnRequest;
                ViewData["action"] = idp.SingleSignOnServiceUrl;

                // Save the IdP label and SPID request id as a cookie
                HttpCookie cookie = Request.Cookies.Get(SPID_COOKIE) ?? new HttpCookie(SPID_COOKIE);
                cookie.Values["IdPName"] = idpName;
                cookie.Values["SpidAuthnRequestId"] = spidAuthnRequestId;
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
            string idpName;
            string subjectNameId;
            string authnStatementSessionIndex;

            // Try to get Authentication data from cookie
            HttpCookie cookie = Request.Cookies[SPID_COOKIE];

            if (cookie == null)
            {
                // End the session
                Session["AppUser"] = null;

                log.Error("Error on HomeController LogoutRequest method: Impossibile recuperare i dati della sessione (cookie scaduto)");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (cookie scaduto).";
                return View("Error");
            }

            idpName = cookie["IdPName"];
            subjectNameId = cookie["SubjectNameId"];
            authnStatementSessionIndex = cookie["AuthnStatementSessionIndex"];

            // Remove the cookie
            cookie.Values["IdPName"] = string.Empty;
            cookie.Values["SpidAuthnRequestId"] = string.Empty;
            cookie.Values["SpidLogoutRequestId"] = string.Empty;
            cookie.Values["SubjectNameId"] = string.Empty;
            cookie.Values["AuthnStatementSessionIndex"] = string.Empty;
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);

            // End the session
            Session["AppUser"] = null;

            if (string.IsNullOrWhiteSpace(idpName) ||
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
                IdentityProvider idp = IdentityProvidersList.GetIdpFromIdPName(idpName);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    ConfigurationManager.AppSettings["SPID_CERTIFICATE_NAME"],
                    validOnly: false);

                // Create the signed SAML logout request
                var spidLogoutRequest = SamlHelper.BuildLogoutPostRequest(
                    uuid: logoutRequestId,
                    consumerServiceURL: ConfigurationManager.AppSettings["SPID_DOMAIN_VALUE"],
                    certificate: certificate,
                    identityProvider: idp,
                    subjectNameId: subjectNameId,
                    authnStatementSessionIndex: authnStatementSessionIndex);

                ViewData["data"] = spidLogoutRequest;
                ViewData["action"] = idp.SingleLogoutServiceUrl;

                // Save the IdP label and SPID request id as a cookie
                cookie = new HttpCookie(SPID_COOKIE);
                cookie.Values["IdPName"] = idpName;
                cookie.Values["SpidLogoutRequestId"] = logoutRequestId;
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

    }
}